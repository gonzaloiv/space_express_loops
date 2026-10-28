using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFixedSearch
    {
        private const int Samples = 32;
        private const float CostFactor = 3f;
        private const float MinDelta = 0.1f;
        private const float GapFactor = 0.25f;

        public static bool TryFind(
            PuzzleData puzzle, PuzzleBrief brief, PuzzleSimSettings settings,
            out PuzzleZone[] regions, out PuzzleSolution solution)
        {
            regions = null;
            solution = default;
            if (brief.Moves != 2)
                return false;

            PuzzleZone a = Region(brief.FixedZoneA, brief.FixedZoneSize, settings);
            PuzzleZone b = Region(brief.FixedZoneB, brief.FixedZoneSize, settings);
            if (!PuzzleZoneCandidates.ContainsSource(a, puzzle)
                || !PuzzleZoneCandidates.ContainsSource(b, puzzle))
                return false;

            if (a.Overlaps(b, GapFactor))
                return false;

            regions = new[] { a, b };
            if (!TrySolve(puzzle, settings, regions, out solution))
            {
                regions = null;
                return false;
            }

            return true;
        }

        public static bool TrySolve(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleZone[] regions,
            out PuzzleSolution solution)
        {
            solution = default;
            if (regions == null || regions.Length < 2)
                return false;

            float[] scales = PuzzleZoneCandidates.Scales(settings, Samples);
            PuzzleZone[] zones = new PuzzleZone[regions.Length];
            PuzzleSolution best = default;
            float bestCost = float.MaxValue;
            bool found = false;
            Search(puzzle, settings, regions, scales, 0, zones, ref best, ref bestCost, ref found);
            solution = best;
            return found;
        }

        public static bool HasShorter(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleZone[] regions, PuzzleSolution found)
        {
            float budget = found.EntropySpent * CostFactor;
            float[] scales = PuzzleZoneCandidates.Scales(settings, Samples);
            for (int r = 0; r < regions.Length; r++)
            {
                for (int s = 0; s < scales.Length; s++)
                {
                    PuzzleZone zone = WithScale(regions[r], scales[s]);
                    if (PuzzleZoneRun.TryArrive(puzzle, settings, zone, out PuzzleSolution single)
                        && PuzzleWindow.Contains(puzzle, single.ShipTime)
                        && single.EntropySpent <= budget)
                        return true;
                }
            }

            return false;
        }

        private static void Search(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleZone[] regions, float[] scales,
            int depth, PuzzleZone[] zones, ref PuzzleSolution best, ref float bestCost, ref bool found)
        {
            if (depth == regions.Length)
            {
                PuzzleZone[] leaf = new PuzzleZone[zones.Length];
                for (int i = 0; i < zones.Length; i++)
                    leaf[i] = zones[i];

                if (!PuzzleZoneRun.TryArrive(puzzle, settings, leaf, out PuzzleSolution candidate))
                    return;

                if (!PuzzleWindow.Contains(puzzle, candidate.ShipTime))
                    return;

                if (found && candidate.EntropySpent >= bestCost)
                    return;

                best = candidate;
                bestCost = candidate.EntropySpent;
                found = true;
                return;
            }

            for (int i = 0; i < scales.Length; i++)
            {
                if (Mathf.Abs(scales[i] - 1f) < MinDelta)
                    continue;

                zones[depth] = WithScale(regions[depth], scales[i]);
                Search(puzzle, settings, regions, scales, depth + 1, zones, ref best, ref bestCost, ref found);
            }
        }

        private static PuzzleZone Region(Vector3 centre, float size, PuzzleSimSettings settings) =>
            PuzzleZoneCandidates.Zone(centre, size, 1f, settings.ZoneFalloff);

        private static PuzzleZone WithScale(PuzzleZone region, float scale) => new()
        {
            centre = region.centre,
            halfExtents = region.halfExtents,
            scale = scale,
            falloff = region.falloff
        };
    }
}
