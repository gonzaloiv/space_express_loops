using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSolutionSearch
    {
        public const int DefaultSamples = 32;

        private const int MinSamples = 2;
        private const int SizeSamples = 3;
        private const int CoarseSamples = 24;
        private const int MaxCentres = 8;
        private const float MergeDistance = 0.0001f;
        private const float CostEpsilon = 0.0001f;

        public static bool TrySelect(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleBrief brief, out PuzzleSolution solution)
        {
            if (!brief.SupportsSingleZone)
            {
                solution = default;
                return false;
            }

            if (!brief.Translatable)
                return TrySelectAtCentre(puzzle, settings, puzzle.zoneCentre, out solution);

            return TrySelectMovable(puzzle, settings, brief, out solution);
        }

        private static bool TrySelectAtCentre(
            PuzzleData puzzle, PuzzleSimSettings settings, Vector3 centre, out PuzzleSolution solution)
        {
            PuzzleSolution[] found = Scan(puzzle, settings, centre, DefaultSamples);
            solution = Best(found, puzzle);
            return found.Length > 0;
        }

        private static bool TrySelectMovable(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleBrief brief, out PuzzleSolution solution)
        {
            CentreHit hit = BestCentre(puzzle, settings, Centres(puzzle, brief.MaxCubeSize));
            if (!hit.Found)
            {
                solution = default;
                return false;
            }

            PuzzleSolution[] fine = Scan(puzzle, settings, hit.Centre, DefaultSamples);
            solution = fine.Length > 0 ? Best(fine, puzzle) : hit.Solution;
            return true;
        }

        private static CentreHit BestCentre(
            PuzzleData puzzle, PuzzleSimSettings settings, Vector3[] centres)
        {
            CentreHit hit = default;
            for (int i = 0; i < centres.Length; i++)
            {
                PuzzleSolution candidate = Best(Scan(puzzle, settings, centres[i], CoarseSamples), puzzle);
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
            float[] sizes = Sizes(puzzle);
            float[] scales = Scales(settings, Mathf.Max(MinSamples, samples));
            for (int s = 0; s < sizes.Length; s++)
            {
                for (int i = 0; i < scales.Length; i++)
                {
                    PuzzleZone zone = Zone(centre, sizes[s], scales[i], settings.ZoneFalloff);
                    SpaceShapingSession session = new(SpaceShapingSetup.Build(puzzle, settings));
                    if (Arrives(session, puzzle, zone, out PuzzleSolution solution))
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

        private static Vector3[] Centres(PuzzleData puzzle, float maxCubeSize)
        {
            List<Vector3> centres = new();
            AddSources(centres, puzzle.sources);
            AddPairs(centres, puzzle.sources, maxCubeSize);
            centres.Sort((a, b) => RunDistance(a, puzzle).CompareTo(RunDistance(b, puzzle)));
            return Trim(centres, MaxCentres);
        }

        private static void AddSources(List<Vector3> centres, PuzzleSource[] sources)
        {
            for (int i = 0; i < sources.Length; i++)
                Add(centres, sources[i].home);
        }

        private static void AddPairs(List<Vector3> centres, PuzzleSource[] sources, float maxCubeSize)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                for (int j = i + 1; j < sources.Length; j++)
                    AddPair(centres, sources[i].home, sources[j].home, maxCubeSize);
            }
        }

        private static void AddPair(List<Vector3> centres, Vector3 a, Vector3 b, float maxCubeSize)
        {
            if (Vector3.Distance(a, b) > maxCubeSize)
                return;

            Add(centres, (a + b) * 0.5f);
        }

        private static void Add(List<Vector3> centres, Vector3 centre)
        {
            for (int i = 0; i < centres.Count; i++)
            {
                if (Vector3.Distance(centres[i], centre) < MergeDistance)
                    return;
            }

            centres.Add(centre);
        }

        private static float RunDistance(Vector3 centre, PuzzleData puzzle) =>
            PuzzleRunGeometry.DistanceToSegment(centre, puzzle.origin, puzzle.destination);

        private static Vector3[] Trim(List<Vector3> centres, int max)
        {
            int count = Mathf.Min(centres.Count, max);
            Vector3[] trimmed = new Vector3[count];
            for (int i = 0; i < count; i++)
                trimmed[i] = centres[i];

            return trimmed;
        }

        private static float[] Sizes(PuzzleData puzzle)
        {
            float min = Mathf.Max(puzzle.cubeSizeMin, 0.01f);
            float max = Mathf.Max(puzzle.cubeSizeMax, min);
            if (max - min < 0.0001f)
                return new[] { min };

            float[] sizes = new float[SizeSamples];
            for (int i = 0; i < sizes.Length; i++)
                sizes[i] = Mathf.Lerp(min, max, i / (float)(sizes.Length - 1));

            return sizes;
        }

        private static float[] Scales(PuzzleSimSettings settings, int count)
        {
            float[] scales = new float[count];
            for (int i = 0; i < count; i++)
                scales[i] = Mathf.Lerp(settings.MinScaleFactor, settings.MaxScaleFactor, i / (float)(count - 1));

            System.Array.Sort(scales, (a, b) => Mathf.Abs(a - 1f).CompareTo(Mathf.Abs(b - 1f)));
            return scales;
        }

        private static PuzzleZone Zone(Vector3 centre, float size, float scale, float falloff) => new()
        {
            centre = centre,
            halfExtents = Vector3.one * (size * 0.5f),
            scale = scale,
            falloff = falloff
        };

        private static bool Arrives(
            SpaceShapingSession session, PuzzleData puzzle, PuzzleZone zone, out PuzzleSolution solution)
        {
            session.ApplyZone(zone.ToSpaceZone());
            TrajectoryResult result = session.Simulate(
                session.BuildField(), new TrajectoryIntegrator(), new ShipLaunch());
            solution = new PuzzleSolution
            {
                Zones = new[] { zone },
                ShipTime = result.ShipTime,
                EntropySpent = session.EntropySpent
            };

            return result.Outcome == TrajectoryOutcome.Arrive && WithinWindow(result, puzzle);
        }

        private static bool WithinWindow(TrajectoryResult result, PuzzleData puzzle) =>
            result.ShipTime >= puzzle.tmin && result.ShipTime <= puzzle.tmax;

        private struct CentreHit
        {
            public Vector3 Centre;
            public PuzzleSolution Solution;
            public bool Found;
        }
    }
}
