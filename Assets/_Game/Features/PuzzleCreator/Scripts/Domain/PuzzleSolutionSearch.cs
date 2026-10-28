using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSolutionSearch
    {
        private const int MinSamples = 2;
        private const float CostEpsilon = 0.0001f;

        public static bool TrySelect(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleBrief brief, out PuzzleSolution solution)
        {
            if (!brief.SupportsSingleZone)
            {
                solution = default;
                return false;
            }

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
                    if (PuzzleZoneRun.TryArrive(puzzle, settings, zone, out PuzzleSolution solution)
                        && WithinWindow(solution.ShipTime, puzzle))
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

        private static bool Precedes(PuzzleSolution a, PuzzleSolution b, PuzzleData puzzle)
        {
            if (a.EntropySpent < b.EntropySpent - CostEpsilon)
                return true;

            if (a.EntropySpent > b.EntropySpent + CostEpsilon)
                return false;

            float centre = (puzzle.tmin + puzzle.tmax) * 0.5f;
            return Mathf.Abs(a.ShipTime - centre) < Mathf.Abs(b.ShipTime - centre);
        }

        private static bool WithinWindow(float shipTime, PuzzleData puzzle) =>
            shipTime >= puzzle.tmin && shipTime <= puzzle.tmax;

        private struct CentreHit
        {
            public Vector3 Centre;
            public PuzzleSolution Solution;
            public bool Found;
        }
    }
}
