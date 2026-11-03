using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSolutionSearch
    {
        private const int MinSamples = 2;

        public static bool TrySelect(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleBrief brief, out PuzzleSolution solution)
        {
            if (!brief.SupportsZones)
            {
                solution = default;
                return false;
            }

            PuzzleSolutionContext context =
                PuzzleSolutionContext.Create(puzzle, settings, brief.Mechanism);
            return TrySelect(context, brief, out solution);
        }

        private static bool TrySelect(
            PuzzleSolutionContext context, PuzzleBrief brief, out PuzzleSolution solution)
        {
            if (brief.Zones > 1)
                return PuzzleBeamSearch.TrySelect(context, brief, out solution);

            CentreHit hit = BestCentre(
                context, PuzzleZoneCandidates.Centres(context.Puzzle, context.Puzzle.cubeSizeMax));
            if (!hit.Found)
            {
                solution = default;
                return false;
            }

            PuzzleSolution[] fine = Scan(
                context, hit.Centre, PuzzleZoneCandidates.DefaultSamples);
            solution = fine.Length > 0 ? Best(fine) : hit.Solution;
            return true;
        }

        public static bool TryFindAtWidth(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleBrief brief, float width,
            out PuzzleSolution solution)
        {
            PuzzleSolutionContext context =
                PuzzleSolutionContext.Create(puzzle, settings, PuzzleMechanism.Any);
            if (brief.Zones <= 1)
                return TryBestSingleAtWidth(context, width, out solution);

            return PuzzleBeamSearch.TrySelect(context, brief, width, out solution);
        }

        private static bool TryBestSingleAtWidth(
            PuzzleSolutionContext context, float width, out PuzzleSolution solution)
        {
            solution = default;
            Vector3[] centres = PuzzleZoneCandidates.Centres(
                context.Puzzle, Mathf.Max(width, 0.01f));
            float[] sizes = { width };
            float[] scales = PuzzleZoneCandidates.Scales(
                context.Settings, PuzzleZoneCandidates.DefaultSamples);
            for (int c = 0; c < centres.Length; c++)
            {
                PuzzleSolution candidate = BestEntropyAtCentre(
                    context, centres[c], sizes, scales);
                if (candidate.Zones != null
                    && (solution.Zones == null || candidate.EntropySpent < solution.EntropySpent))
                    solution = candidate;
            }

            return solution.Zones != null;
        }

        private static PuzzleSolution BestEntropyAtCentre(
            PuzzleSolutionContext context, Vector3 centre, float[] sizes, float[] scales)
        {
            PuzzleSolution best = default;
            List<PuzzleZone> zones = PuzzleZoneCandidates.Build(
                centre, sizes, scales, context.Settings.ZoneFalloff);
            for (int i = 0; i < zones.Count; i++)
            {
                if (!TryZone(context, zones[i], out PuzzleSolution candidate))
                    continue;

                if (best.Zones == null || candidate.EntropySpent < best.EntropySpent)
                    best = candidate;
            }

            return best;
        }

        private static bool TryZone(
            PuzzleSolutionContext context, PuzzleZone zone, out PuzzleSolution solution)
        {
            solution = default;
            if (PuzzleZoneCandidates.ContainsEndpoint(zone, context.Puzzle))
                return false;

            if (!PuzzleZoneCandidates.CoversSource(zone, context.Puzzle))
                return false;

            if (!PuzzleZoneRun.Simulate(context, new[] { zone }, out solution, out _))
                return false;

            return context.Matches(solution.Kind);
        }

        private static CentreHit BestCentre(
            PuzzleSolutionContext context, Vector3[] centres)
        {
            CentreHit hit = default;
            for (int i = 0; i < centres.Length; i++)
            {
                PuzzleSolution candidate = Best(
                    Scan(context, centres[i], PuzzleZoneCandidates.CoarseSamples));
                if (candidate.Zones == null)
                    continue;

                if (!hit.Found || Precedes(candidate, hit.Solution))
                {
                    hit.Found = true;
                    hit.Centre = centres[i];
                    hit.Solution = candidate;
                }
            }

            return hit;
        }

        private static PuzzleSolution[] Scan(
            PuzzleSolutionContext context, Vector3 centre, int samples)
        {
            List<PuzzleSolution> found = new();
            float[] sizes = PuzzleZoneCandidates.Sizes(context.Puzzle);
            float[] scales = PuzzleZoneCandidates.Scales(
                context.Settings, Mathf.Max(MinSamples, samples));
            List<PuzzleZone> zones = PuzzleZoneCandidates.Build(
                centre, sizes, scales, context.Settings.ZoneFalloff);
            for (int i = 0; i < zones.Count; i++)
            {
                if (TryZone(context, zones[i], out PuzzleSolution solution))
                    found.Add(solution);
            }

            return found.ToArray();
        }

        private static PuzzleSolution Best(PuzzleSolution[] solutions)
        {
            if (solutions.Length == 0)
                return default;

            PuzzleSolution best = solutions[0];
            for (int i = 1; i < solutions.Length; i++)
            {
                if (Precedes(solutions[i], best))
                    best = solutions[i];
            }

            return best;
        }

        private static bool Precedes(PuzzleSolution a, PuzzleSolution b) =>
            PuzzleSolutionOrdering.Precedes(a, b);

        private struct CentreHit
        {
            public Vector3 Centre;
            public PuzzleSolution Solution;
            public bool Found;
        }
    }
}
