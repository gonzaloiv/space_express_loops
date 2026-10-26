using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBake
    {
        private const float RunMargin = 0.15f;
        private const float StarBudgetFactor = 3f;
        private const int SearchSamples = 64;

        public static void Apply(PuzzleData puzzle, PuzzlePlanetListAsset planets, PuzzleSimSettings settings)
        {
            puzzle.sources = PuzzleSourceBridge.ToSources(planets.planets);
            puzzle.distance = RunLength(planets.planets, settings.CaptureRadius);
        }

        public static PuzzleData[] BakeVariants(
            PuzzlePlanetListAsset planets, PuzzleSimSettings settings, float windowTightness, int variants)
        {
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            Apply(field, planets, settings);
            if (!StartMisses(field, settings))
            {
                Object.DestroyImmediate(field);
                return System.Array.Empty<PuzzleData>();
            }

            PuzzleSolution[] solutions = PuzzleSolutionSearch.Find(field, settings, SearchSamples, variants);
            List<PuzzleData> baked = BakeEach(field, solutions, windowTightness, settings);
            Object.DestroyImmediate(field);
            return baked.ToArray();
        }

        private static bool StartMisses(PuzzleData field, PuzzleSimSettings settings) =>
            PuzzleVerifier.Verify(field, settings).StartOutcome == TrajectoryOutcome.Miss;

        private static List<PuzzleData> BakeEach(
            PuzzleData field, PuzzleSolution[] solutions, float windowTightness, PuzzleSimSettings settings)
        {
            List<PuzzleData> baked = new();
            for (int i = 0; i < solutions.Length; i++)
            {
                PuzzleData variant = Variant(field, solutions[i], windowTightness);
                if (PuzzleVerifier.Verify(variant, settings).Solved)
                    baked.Add(variant);
                else
                    Object.DestroyImmediate(variant);
            }

            return baked;
        }

        private static PuzzleData Variant(PuzzleData field, PuzzleSolution solution, float windowTightness)
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.distance = field.distance;
            puzzle.sources = field.sources;
            puzzle.solution = solution.Zones;
            puzzle.tmin = solution.ShipTime - windowTightness * 0.5f;
            puzzle.tmax = solution.ShipTime + windowTightness * 0.5f;
            puzzle.entropyBudget = solution.EntropySpent * StarBudgetFactor;
            return puzzle;
        }

        private static float RunLength(PuzzleFieldPlanet[] planets, float captureRadius)
        {
            float outer = 0f;
            for (int i = 0; i < planets.Length; i++)
                outer = Mathf.Max(outer, planets[i].position.magnitude + planets[i].radius);

            return 2f * (outer + captureRadius + RunMargin);
        }
    }
}
