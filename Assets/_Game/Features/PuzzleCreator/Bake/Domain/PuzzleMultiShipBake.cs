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
            PuzzleMultiShipSearch search = PuzzleMultiShipSearch.Prepare(planets, settings, brief, 0);
            PuzzleMultiShipResult result = search.Confirm(search.Run());
            search.Dispose();
            return result;
        }

        // The seam the background search hands back to: the zone and runs came off the editor thread,
        // and this main-thread step selects the ships, re-checks the rescue and builds the variant.
        public static PuzzleMultiShipResult ConfirmZone(
            PuzzleData field, FleetField fleet, FleetZoneResult found, PuzzleBrief brief)
        {
            PuzzleShip[] ships = Select(fleet, found.Rescued, brief);
            if (ships == null || !found.Scan.Rescues(found.Zone, ships))
                return new PuzzleMultiShipResult { Status = PuzzleBakeStatus.NoSolution };

            // A movable fleet's frame is chosen by PuzzleStartFrame.Place, which already keeps it off
            // endpoints; a fixed fleet ships the zone itself, which PuzzleVerifier.Fits checks.
            return Confirm(field, fleet.Settings, brief, found.Zone, ships);
        }

        private static PuzzleMultiShipResult Confirm(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief,
            PuzzleZone zone, PuzzleShip[] ships)
        {
            PuzzleData variant = Variant(field, settings, zone, ships, brief);
            PuzzleVerification check = PuzzleVerifier.Verify(variant, new[] { zone }, settings);
            PuzzleBakeStatus rejected = Reject(variant, settings, brief, check);
            if (rejected != PuzzleBakeStatus.Baked)
            {
                Object.DestroyImmediate(variant);
                return new PuzzleMultiShipResult { Status = rejected, Kind = check.Kind };
            }

            return new PuzzleMultiShipResult
            {
                Puzzle = variant,
                Solution = zone,
                Status = PuzzleBakeStatus.Baked,
                Kind = check.Kind
            };
        }

        private static PuzzleBakeStatus Reject(
            PuzzleData variant, PuzzleSimSettings settings, PuzzleBrief brief, PuzzleVerification check)
        {
            if (!check.Solved)
                return PuzzleBakeStatus.Unverified;

            if (brief.Mechanism != PuzzleMechanism.Any
                && !PuzzleSolutionClassifier.Matches(brief.Mechanism, check.Kind))
                return PuzzleBakeStatus.WrongRole;

            if (brief.SunRule == PuzzleSunRule.NoSunAnswer
                && PuzzleSunRuleCheck.SunAnswers(variant, settings))
                return PuzzleBakeStatus.SunAnswers;

            return PuzzleBakeStatus.Baked;
        }

        // Take the first runs the brief's relation allows between every pair of ships.
        private static PuzzleShip[] Select(
            FleetField fleet, List<PuzzleShipRescue> rescued, PuzzleBrief brief)
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
                    captureRadius = fleet.CaptureRadiusAt(rescued[i].Destination)
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
            PuzzleData field, PuzzleSimSettings settings, PuzzleZone zone, PuzzleShip[] ships,
            PuzzleBrief brief)
        {
            PuzzleData puzzle = Object.Instantiate(field);
            puzzle.ships = ships;
            puzzle.zones = 1;
            puzzle.origin = ships[0].origin;
            puzzle.destination = ships[0].destination;
            puzzle.captureRadius = ships[0].captureRadius;
            puzzle.zoneCentre = Centre(ships);
            puzzle.entropyBudget = Mathf.Abs(zone.scale - 1f) * PuzzleGrade.BudgetFactor;
            puzzle.frames = Frames(puzzle, settings, zone, brief);
            return puzzle;
        }

        // The bounding box of every ship's endpoints; the field's zone centre.
        public static Vector3 Centre(PuzzleShip[] ships)
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

        // A movable fleet starts at the contract's spot (run midpoint or field centre) and takes the
        // nearest spot where scaling alone fails, so moving the cube is the lesson. A fixed fleet
        // ships the solved zone itself.
        private static PuzzleFrame[] Frames(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleZone zone, PuzzleBrief brief)
        {
            if (!brief.Translatable)
                return new[] { new PuzzleFrame { centre = zone.centre, size = zone.Width } };

            Vector3 preferred = PuzzleFieldBake.StartSpot(puzzle, brief.StartRule);
            Vector3 centre = PuzzleStartFrame.Place(puzzle, settings, preferred, zone.Width);
            return new[] { new PuzzleFrame { centre = centre, size = zone.Width } };
        }
    }
}
