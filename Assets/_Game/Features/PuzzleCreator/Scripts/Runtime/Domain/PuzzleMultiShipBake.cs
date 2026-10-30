using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Bake a multi-ship puzzle from the inverted search: find a zone whose field-driven rescue set holds
    // the brief's ship count across a scale plateau, take that many runs as the ships, and ship the zone
    // as a movable start frame. The result is confirmed by an independent N-run re-check.
    public static class PuzzleMultiShipBake
    {
        private const int ScaleSamples = 13;

        public static PuzzleMultiShipResult Bake(
            PuzzlePlanetListAsset planets, PuzzleSimSettings settings, PuzzleBrief brief)
        {
            if (!brief.IsMulti)
                return new PuzzleMultiShipResult { Status = PuzzleBakeStatus.UnsupportedBrief };

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
            Probe probe = new(field, settings, brief);
            if (!probe.Find(out PuzzleZone zone, out List<PuzzleShipRescue> rescued))
                return new PuzzleMultiShipResult { Status = PuzzleBakeStatus.NoSolution };

            PuzzleShip[] ships = Select(field, rescued, brief);
            if (ships == null || !probe.Scan.Rescues(zone, ships))
                return new PuzzleMultiShipResult { Status = PuzzleBakeStatus.NoSolution };

            return new PuzzleMultiShipResult
            {
                Puzzle = Variant(field, zone, ships, brief),
                Solution = zone,
                Status = PuzzleBakeStatus.Baked
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
                    captureRadius = Capture(field, rescued[i].Destination)
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
        internal static bool Allowed(
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

        private static float Capture(PuzzleData field, Vector3 destination)
        {
            for (int i = 0; i < field.sources.Length; i++)
            {
                if ((field.sources[i].home - destination).sqrMagnitude < 1e-8f)
                    return field.sources[i].scale * 0.5f;
            }

            return 0f;
        }

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

        private sealed class Probe
        {
            private readonly PuzzleShipScan scan;
            private readonly PuzzleData field;
            private readonly PuzzleSimSettings settings;
            private readonly float[] scales;
            private readonly int required;
            private readonly PuzzleShipRelation relation;

            public Probe(PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief)
            {
                this.field = field;
                this.settings = settings;
                required = brief.Ships;
                relation = brief.Relation;
                scan = new PuzzleShipScan(field, settings, 0f);
                scales = Linear(settings, ScaleSamples);
            }

            public PuzzleShipScan Scan => scan;

            public bool Find(out PuzzleZone zone, out List<PuzzleShipRescue> rescued)
            {
                Candidate best = default;
                Vector3[] centres = PuzzleZoneCandidates.Centres(field, field.cubeSizeMax);
                float[] sizes = PuzzleZoneCandidates.Sizes(field);

                for (int c = 0; c < centres.Length; c++)
                {
                    for (int s = 0; s < sizes.Length; s++)
                        Consider(centres[c], sizes[s], ref best);
                }

                zone = best.Zone;
                rescued = best.Rescued;
                return best.Count >= required;
            }

            private void Consider(Vector3 centre, float size, ref Candidate best)
            {
                List<PuzzleShipRescue>[] row = Row(centre, size);
                for (int k = 0; k < row.Length; k++)
                {
                    if (!Plateau(row, k))
                        continue;

                    int distinct = Distinct(row[k]);
                    if (distinct <= best.Count)
                        continue;

                    best.Count = distinct;
                    best.Zone = Zone(centre, size, scales[k]);
                    best.Rescued = row[k];
                }
            }

            private List<PuzzleShipRescue>[] Row(Vector3 centre, float size)
            {
                List<PuzzleShipRescue>[] row = new List<PuzzleShipRescue>[scales.Length];
                for (int k = 0; k < scales.Length; k++)
                {
                    PuzzleZone zone = Zone(centre, size, scales[k]);
                    if (PuzzleZoneCandidates.CoversSource(zone, field))
                        row[k] = scan.RescuedFixed(zone);
                }

                return row;
            }

            private bool Plateau(List<PuzzleShipRescue>[] row, int k)
            {
                if (row[k] == null || Distinct(row[k]) < required)
                    return false;

                if (k > 0 && row[k - 1] != null && Distinct(row[k - 1]) >= required)
                    return true;

                return k + 1 < row.Length && row[k + 1] != null && Distinct(row[k + 1]) >= required;
            }

            // How many relation-allowed ships a rescue set can supply.
            private int Distinct(List<PuzzleShipRescue> rescued)
            {
                int distinct = 0;
                for (int i = 0; i < rescued.Count; i++)
                {
                    if (Fresh(rescued, i))
                        distinct++;
                }

                return distinct;
            }

            private bool Fresh(List<PuzzleShipRescue> rescued, int index)
            {
                for (int i = 0; i < index; i++)
                {
                    if (!Allowed(
                        relation, rescued[i].Origin, rescued[i].Destination,
                        rescued[index].Origin, rescued[index].Destination))
                        return false;
                }

                return true;
            }

            private PuzzleZone Zone(Vector3 centre, float size, float scale) =>
                PuzzleZoneCandidates.Zone(centre, size, scale, settings.ZoneFalloff);

            private struct Candidate
            {
                public int Count;
                public PuzzleZone Zone;
                public List<PuzzleShipRescue> Rescued;
            }

            private static float[] Linear(PuzzleSimSettings settings, int samples)
            {
                float[] scales = new float[samples];
                for (int i = 0; i < samples; i++)
                {
                    scales[i] = Mathf.Lerp(
                        settings.MinScaleFactor, settings.MaxScaleFactor, i / (float)(samples - 1));
                }

                return scales;
            }
        }
    }
}
