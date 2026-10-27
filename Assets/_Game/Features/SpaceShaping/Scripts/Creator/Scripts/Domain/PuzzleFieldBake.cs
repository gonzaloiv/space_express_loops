using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBake
    {
        private const float RunMargin = 0.02f;
        private const float StarBudgetFactor = 3f;
        private const float BareMissMargin = 0.9f;
        private const int SearchSamples = 64;

        public static void Apply(PuzzleData puzzle, PuzzlePlanetListAsset planets, PuzzleSimSettings settings)
        {
            puzzle.sources = PuzzleSourceBridge.ToSources(planets.planets);
            puzzle.distance = RunLength(planets.planets, settings.CaptureRadius);
            puzzle.launchSpeed = LaunchSpeed(puzzle.distance, settings);
        }

        public static PuzzleData[] BakeVariants(
            PuzzlePlanetListAsset planets, PuzzleSimSettings settings, float windowTightness, int variants) =>
            BakeVariants(planets, settings, windowTightness, variants, out _);

        public static PuzzleData[] BakeVariants(
            PuzzlePlanetListAsset planets, PuzzleSimSettings settings, float windowTightness, int variants,
            out PuzzleBakeStatus status)
        {
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            Apply(field, planets, settings);
            PuzzleData[] baked = BakeField(field, settings, windowTightness, variants, out status);
            Object.DestroyImmediate(field);
            return baked;
        }

        private static PuzzleData[] BakeField(
            PuzzleData field, PuzzleSimSettings settings, float windowTightness, int variants,
            out PuzzleBakeStatus status)
        {
            status = PuzzleBakeStatus.StartNotMiss;
            if (!StartMisses(field, settings))
                return System.Array.Empty<PuzzleData>();

            PuzzleSolution[] reference = PuzzleSolutionSearch.Arrivals(field, settings, SearchSamples, 1);
            if (reference.Length == 0)
            {
                status = PuzzleBakeStatus.NoSolution;
                return System.Array.Empty<PuzzleData>();
            }

            SetWindow(field, reference[0], windowTightness);
            PuzzleSolution[] solutions = PuzzleSolutionSearch.Find(field, settings, SearchSamples, variants);
            if (solutions.Length == 0)
            {
                status = PuzzleBakeStatus.NoSolution;
                return System.Array.Empty<PuzzleData>();
            }

            List<PuzzleData> baked = BakeEach(field, solutions, settings);
            status = baked.Count > 0 ? PuzzleBakeStatus.Baked : PuzzleBakeStatus.Unverified;
            return baked.ToArray();
        }

        private static void SetWindow(PuzzleData field, PuzzleSolution reference, float windowTightness)
        {
            field.tmin = reference.ShipTime - windowTightness * 0.5f;
            field.tmax = reference.ShipTime + windowTightness * 0.5f;
            field.entropyBudget = reference.EntropySpent * StarBudgetFactor;
        }

        private static bool StartMisses(PuzzleData field, PuzzleSimSettings settings) =>
            PuzzleVerifier.Verify(field, settings).StartOutcome == TrajectoryOutcome.Miss;

        private static List<PuzzleData> BakeEach(
            PuzzleData field, PuzzleSolution[] solutions, PuzzleSimSettings settings)
        {
            List<PuzzleData> baked = new();
            for (int i = 0; i < solutions.Length; i++)
            {
                PuzzleData variant = Variant(field, solutions[i]);
                if (PuzzleVerifier.Verify(variant, settings).Solved)
                    baked.Add(variant);
                else
                    Object.DestroyImmediate(variant);
            }

            return baked;
        }

        private static PuzzleData Variant(PuzzleData field, PuzzleSolution solution)
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.distance = field.distance;
            puzzle.launchSpeed = field.launchSpeed;
            puzzle.sources = field.sources;
            puzzle.solution = solution.Zones;
            puzzle.tmin = field.tmin;
            puzzle.tmax = field.tmax;
            puzzle.entropyBudget = field.entropyBudget;
            return puzzle;
        }

        private static float RunLength(PuzzleFieldPlanet[] planets, float captureRadius)
        {
            float outer = 0f;
            for (int i = 0; i < planets.Length; i++)
                outer = Mathf.Max(outer, planets[i].position.magnitude + planets[i].radius);

            return 2f * (outer + captureRadius + RunMargin);
        }

        private static float LaunchSpeed(float run, PuzzleSimSettings settings)
        {
            float budget = settings.MaxSteps * settings.StepSecs;
            return budget > 0f ? BareMissMargin * run / budget : settings.LaunchSpeed;
        }
    }
}
