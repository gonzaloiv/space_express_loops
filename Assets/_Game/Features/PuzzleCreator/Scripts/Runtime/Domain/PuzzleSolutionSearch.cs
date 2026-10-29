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
            if (!brief.SupportsMoves)
            {
                solution = default;
                return false;
            }

            if (brief.Moves > 1)
                return PuzzleBeamSearch.TrySelect(puzzle, settings, brief, out solution);

            if (brief.OffPath)
                return PuzzleClockSearch.TrySelect(puzzle, settings, brief, out solution);

            CentreHit hit = BestCentre(
                puzzle, settings, PuzzleZoneCandidates.Centres(puzzle, brief.MaxCubeSize));
            if (!hit.Found)
            {
                solution = default;
                return false;
            }

            PuzzleSolution[] fine = Scan(
                puzzle, settings, hit.Centre, PuzzleZoneCandidates.DefaultSamples);
            solution = fine.Length > 0 ? Best(fine, puzzle) : hit.Solution;
            return true;
        }

        internal static bool TryFindAtWidth(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleBrief brief, float width,
            out PuzzleSolution solution)
        {
            if (brief.Moves <= 1)
                return TryBestSingleAtWidth(puzzle, settings, width, out solution);

            return PuzzleBeamSearch.TrySelect(puzzle, settings, brief, width, out solution);
        }

        private static bool TryBestSingleAtWidth(
            PuzzleData puzzle, PuzzleSimSettings settings, float width, out PuzzleSolution solution)
        {
            solution = default;
            Vector3[] centres = PuzzleZoneCandidates.Centres(puzzle, Mathf.Max(width, 0.01f));
            float[] sizes = { width };
            float[] scales = PuzzleZoneCandidates.Scales(
                settings, PuzzleZoneCandidates.DefaultSamples);
            for (int c = 0; c < centres.Length; c++)
            {
                PuzzleSolution candidate = BestEntropyAtCentre(
                    puzzle, settings, centres[c], sizes, scales);
                if (candidate.Zones != null
                    && (solution.Zones == null || candidate.EntropySpent < solution.EntropySpent))
                    solution = candidate;
            }

            return solution.Zones != null;
        }

 private static PuzzleSolution BestEntropyAtCentre(
            PuzzleData puzzle, PuzzleSimSettings settings, Vector3 centre, float[] sizes,
            float[] scales)
        {
            PuzzleSolution best = default;
            for (int s = 0; s < sizes.Length; s++)
            {
                for (int i = 0; i < scales.Length; i++)
                {
                    PuzzleZone zone = PuzzleZoneCandidates.Zone(
                        centre, sizes[s], scales[i], settings.ZoneFalloff);
                    if (!TryZone(puzzle, settings, zone, out PuzzleSolution candidate))
                        continue;

                    if (best.Zones == null || candidate.EntropySpent < best.EntropySpent)
                        best = candidate;
                }
            }

            return best;
        }



        private static bool TryZone(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleZone zone,
            out PuzzleSolution solution)
        {
            solution = default;
            if (PuzzleZoneCandidates.ContainsEndpoint(zone, puzzle))
                return false;

            if (!PuzzleZoneCandidates.ContainsSource(zone, puzzle))
                return false;

            return PuzzleZoneRun.TryArrive(puzzle, settings, zone, out solution)
                && PuzzleWindow.Contains(puzzle, solution.ShipTime);
        }

        private static CentreHit BestCentre(
            PuzzleData puzzle, PuzzleSimSettings settings, Vector3[] centres)
        {
            CentreHit hit = default;
            for (int i = 0; i < centres.Length; i++)
            {
                PuzzleSolution candidate = Best(
                    Scan(puzzle, settings, centres[i], PuzzleZoneCandidates.CoarseSamples), puzzle);
                if (candidate.Zones == null)
                    continue;

                if (!hit.Found || Precedes(candidate, hit.Solution, puzzle))
                {
                    hit.Found = true;
                    hit.Centre = centres[i];
                    hit.Solution = candidate;
                }
            }

            return hit;
        }

        private static PuzzleSolution[] Scan(
            PuzzleData puzzle, PuzzleSimSettings settings, Vector3 centre, int samples)
        {
            List<PuzzleSolution> found = new();
            float[] sizes = PuzzleZoneCandidates.Sizes(puzzle);
            float[] scales = PuzzleZoneCandidates.Scales(settings, Mathf.Max(MinSamples, samples));
            for (int s = 0; s < sizes.Length; s++)
            {
                for (int i = 0; i < scales.Length; i++)
                {
                    PuzzleZone zone = PuzzleZoneCandidates.Zone(
                        centre, sizes[s], scales[i], settings.ZoneFalloff);
                    if (TryZone(puzzle, settings, zone, out PuzzleSolution solution))
                        found.Add(solution);
                }
            }

            return found.ToArray();
        }

        private static PuzzleSolution Best(PuzzleSolution[] solutions, PuzzleData puzzle)
        {
            if (solutions.Length == 0)
                return default;

            PuzzleSolution best = solutions[0];
            for (int i = 1; i < solutions.Length; i++)
            {
                if (Precedes(solutions[i], best, puzzle))
                    best = solutions[i];
            }

            return best;
        }

        private static bool Precedes(PuzzleSolution a, PuzzleSolution b, PuzzleData puzzle) =>
            PuzzleSolutionOrdering.Precedes(a, b, PuzzleSolutionOrdering.WindowCentre(puzzle));

        private struct CentreHit
        {
            public Vector3 Centre;
            public PuzzleSolution Solution;
            public bool Found;
        }
    }
}
