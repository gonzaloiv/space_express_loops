using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBake
    {
        private const float StarBudgetFactor = 3f;
        private const float BareMissMargin = 0.9f;
        private const int SearchSamples = 64;
        private const float MinRunFraction = 0.5f;
        private const float MaxRunFactor = 1.5f;

        public static void Apply(PuzzleData puzzle, PuzzlePlanetListAsset planets, PuzzleSimSettings settings)
        {
            puzzle.sources = PuzzleSourceBridge.ToSources(planets.planets);
            if (!TryPickRun(
                planets.planets, settings.RunDistance, planets.clearance,
                out Vector3 origin, out Vector3 destination))
                return;

            puzzle.origin = origin;
            puzzle.destination = destination;
            puzzle.zoneCentre = (origin + destination) * 0.5f;
            puzzle.launchSpeed = LaunchSpeed(settings);
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
            status = PuzzleBakeStatus.NoRun;
            if (field.Distance <= 0f)
                return System.Array.Empty<PuzzleData>();

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

        private static bool TryPickRun(
            PuzzleFieldPlanet[] planets, float targetRun, float clearance,
            out Vector3 origin, out Vector3 destination)
        {
            origin = default;
            destination = default;
            if (planets.Length < 2)
                return false;

            float best = float.MaxValue;
            bool found = false;
            for (int i = 0; i < planets.Length; i++)
            {
                for (int j = i + 1; j < planets.Length; j++)
                {
                    float run = Vector3.Distance(planets[i].position, planets[j].position);
                    if (run < targetRun * MinRunFraction || run > targetRun * MaxRunFactor)
                        continue;

                    if (!CorridorIsClear(planets, i, j, clearance))
                        continue;

                    float delta = Mathf.Abs(run - targetRun);
                    if (delta >= best)
                        continue;

                    best = delta;
                    origin = planets[i].position;
                    destination = planets[j].position;
                    found = true;
                }
            }

            return found;
        }

        private static bool CorridorIsClear(PuzzleFieldPlanet[] planets, int a, int b, float clearance)
        {
            Vector3 from = planets[a].position;
            Vector3 to = planets[b].position;
            for (int k = 0; k < planets.Length; k++)
            {
                if (k == a || k == b)
                    continue;

                if (DistanceToSegment(planets[k].position, from, to) < clearance + planets[k].radius)
                    return false;
            }

            return true;
        }

        private static float DistanceToSegment(Vector3 point, Vector3 from, Vector3 to)
        {
            Vector3 segment = to - from;
            float lengthSq = segment.sqrMagnitude;
            if (lengthSq < 0.0000001f)
                return Vector3.Distance(point, from);

            float t = Mathf.Clamp01(Vector3.Dot(point - from, segment) / lengthSq);
            return Vector3.Distance(point, from + segment * t);
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
            puzzle.origin = field.origin;
            puzzle.destination = field.destination;
            puzzle.zoneCentre = field.zoneCentre;
            puzzle.launchSpeed = field.launchSpeed;
            puzzle.sources = field.sources;
            puzzle.solution = solution.Zones;
            puzzle.tmin = field.tmin;
            puzzle.tmax = field.tmax;
            puzzle.entropyBudget = field.entropyBudget;
            return puzzle;
        }

        private static float LaunchSpeed(PuzzleSimSettings settings) =>
            BareMissMargin * settings.NominalSpeed;
    }
}
