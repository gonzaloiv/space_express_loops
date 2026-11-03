using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Bake a multi-ship puzzle from the inverted search: find a zone whose field-driven rescue set holds
    // the brief's ship count across a scale plateau, take that many runs as the ships, and ship the zone
    // as a movable start frame. The result is confirmed by an independent N-run re-check.
    public static class PuzzleMultiShipBake
    {
        public static PuzzleMultiShipResult Bake(
            PuzzlePlanetListAsset planets, PuzzleSimSettings settings, PuzzleBrief brief)
        {
            if (!brief.IsMulti)
                return new PuzzleMultiShipResult { Status = PuzzleBakeStatus.UnsupportedBrief };

            if (brief.Mechanism != PuzzleMechanism.Any)
                return new PuzzleMultiShipResult { Status = PuzzleBakeStatus.UnsupportedMechanism };

            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            if (!PuzzleFieldBake.Apply(brief, field, planets, settings))
            {
                Object.DestroyImmediate(field);
                return new PuzzleMultiShipResult { Status = PuzzleBakeStatus.NoRun };
            }

            PuzzleMultiShipResult result = BakeField(field, settings, brief);
            Object.DestroyImmediate(field);
            return result;
        }

        private static PuzzleMultiShipResult BakeField(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief)
        {
            PuzzleMultiShipProbe probe = new(field, settings, brief);
            if (!probe.Find(out PuzzleZone zone, out List<PuzzleShipRescue> rescued))
                return new PuzzleMultiShipResult { Status = PuzzleBakeStatus.NoSolution };

            PuzzleShip[] ships = Select(field, rescued, brief);
            if (ships == null || !probe.Scan.Rescues(zone, ships))
                return new PuzzleMultiShipResult { Status = PuzzleBakeStatus.NoSolution };

            return Confirm(field, settings, brief, zone, ships);
        }

        private static PuzzleMultiShipResult Confirm(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief,
            PuzzleZone zone, PuzzleShip[] ships)
        {
            PuzzleData variant = Variant(field, zone, ships, brief);
            PuzzleVerification check = PuzzleVerifier.Verify(variant, new[] { zone }, settings);
            if (!check.Solved)
            {
                Object.DestroyImmediate(variant);
                return new PuzzleMultiShipResult
                {
                    Status = PuzzleBakeStatus.Unverified,
                    Kind = check.Kind
                };
            }

            return new PuzzleMultiShipResult
            {
                Puzzle = variant,
                Solution = zone,
                Status = PuzzleBakeStatus.Baked,
                Kind = check.Kind
            };
        }

        // Take the first runs the brief's relation allows between every pair of ships.
        private static PuzzleShip[] Select(PuzzleData field, List<PuzzleShipRescue> rescued, PuzzleBrief brief)
        {
            PuzzleShip[] ships = new PuzzleShip[brief.Ships];
            int taken = 0;
            for (int i = 0; i < rescued.Count && taken < brief.Ships; i++)
            {
                if (Used(brief.Relation, ships, taken, rescued[i]))
                    continue;

                ships[taken] = new PuzzleShip
                {
                    origin = rescued[i].Origin,
                    destination = rescued[i].Destination,
                    captureRadius = field.CaptureRadiusAt(rescued[i].Destination)
                };
                taken++;
            }

            return taken == brief.Ships ? ships : null;
        }

        private static bool Used(
            PuzzleShipRelation relation, PuzzleShip[] ships, int taken, PuzzleShipRescue rescue)
        {
            for (int i = 0; i < taken; i++)
            {
                if (!Allowed(
                    relation, ships[i].origin, ships[i].destination, rescue.Origin, rescue.Destination))
                    return true;
            }

            return false;
        }

        // How two ships may relate. Fan-out shares an origin, converge shares a destination, and a
        // separate pair shares no planet at all.
        public static bool Allowed(
            PuzzleShipRelation relation, Vector3 origin, Vector3 destination,
            Vector3 otherOrigin, Vector3 otherDestination)
        {
            if (relation == PuzzleShipRelation.FanOut)
                return Same(origin, otherOrigin) && !Same(destination, otherDestination);

            if (relation == PuzzleShipRelation.Converge)
                return Same(destination, otherDestination) && !Same(origin, otherOrigin);

            return !Same(origin, otherOrigin) && !Same(origin, otherDestination)
                && !Same(destination, otherOrigin) && !Same(destination, otherDestination);
        }

        private static bool Same(Vector3 a, Vector3 b) => (a - b).sqrMagnitude < 1e-8f;

        private static PuzzleData Variant(
            PuzzleData field, PuzzleZone zone, PuzzleShip[] ships, PuzzleBrief brief)
        {
            PuzzleData puzzle = Object.Instantiate(field);
            puzzle.ships = ships;
            puzzle.zones = 1;
            puzzle.origin = ships[0].origin;
            puzzle.destination = ships[0].destination;
            puzzle.captureRadius = ships[0].captureRadius;
            puzzle.zoneCentre = Centre(ships);
            puzzle.entropyBudget = Mathf.Abs(zone.scale - 1f) * PuzzleGrade.BudgetFactor;
            puzzle.frames = Frames(puzzle, zone, brief);
            return puzzle;
        }

        private static Vector3 Centre(PuzzleShip[] ships)
        {
            Vector3 min = ships[0].origin;
            Vector3 max = ships[0].origin;
            for (int i = 0; i < ships.Length; i++)
            {
                min = Vector3.Min(min, Vector3.Min(ships[i].origin, ships[i].destination));
                max = Vector3.Max(max, Vector3.Max(ships[i].origin, ships[i].destination));
            }

            return (min + max) * 0.5f;
        }

        private static PuzzleFrame[] Frames(PuzzleData puzzle, PuzzleZone zone, PuzzleBrief brief)
        {
            if (!brief.Translatable)
                return new[] { new PuzzleFrame { centre = zone.centre, size = zone.Width } };

            return new[] { new PuzzleFrame { centre = puzzle.zoneCentre, size = zone.Width } };
        }
    }
}
