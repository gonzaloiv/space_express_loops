using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSolutionSearch
    {
        public const int DefaultSamples = 32;

        private const int MinSamples = 2;

        public static bool TryFind(PuzzleData puzzle, PuzzleSimSettings settings, out PuzzleSolution solution) =>
            TryFind(puzzle, settings, DefaultSamples, out solution);

        public static bool TryFind(
            PuzzleData puzzle, PuzzleSimSettings settings, int samples, out PuzzleSolution solution)
        {
            PuzzleSolution[] found = Find(puzzle, settings, samples, 1);
            solution = found.Length > 0 ? found[0] : default;
            return found.Length > 0;
        }

        public static PuzzleSolution[] Find(
            PuzzleData puzzle, PuzzleSimSettings settings, int samples, int max) =>
            Collect(puzzle, settings, samples, max, true);

        public static PuzzleSolution[] Arrivals(
            PuzzleData puzzle, PuzzleSimSettings settings, int samples, int max) =>
            Collect(puzzle, settings, samples, max, false);

        private static PuzzleSolution[] Collect(
            PuzzleData puzzle, PuzzleSimSettings settings, int samples, int max, bool inWindow)
        {
            List<PuzzleSolution> found = new();
            float[] scales = Scales(settings, Mathf.Max(MinSamples, samples));
            for (int i = 0; i < scales.Length && found.Count < max; i++)
            {
                PuzzleZone zone = Zone(Cube(settings.CubeSize), scales[i], settings.ZoneFalloff);
                SpaceShapingSession session = new(SpaceShapingSetup.Build(puzzle, settings));
                if (Arrives(session, puzzle, zone, inWindow, out PuzzleSolution solution))
                    found.Add(solution);
            }

            return found.ToArray();
        }

        private static float[] Scales(PuzzleSimSettings settings, int count)
        {
            float[] scales = new float[count];
            for (int i = 0; i < count; i++)
                scales[i] = Mathf.Lerp(settings.MinScaleFactor, settings.MaxScaleFactor, i / (float)(count - 1));

            System.Array.Sort(scales, (a, b) => Mathf.Abs(a - 1f).CompareTo(Mathf.Abs(b - 1f)));
            return scales;
        }

        private static Vector3 Cube(float size) => new(size, size, size);

        private static PuzzleZone Zone(Vector3 size, float scale, float falloff) => new()
        {
            centre = Vector3.zero,
            size = size,
            scale = scale,
            falloff = falloff
        };

        private static bool Arrives(
            SpaceShapingSession session, PuzzleData puzzle, PuzzleZone zone, bool inWindow,
            out PuzzleSolution solution)
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

            if (result.Outcome != TrajectoryOutcome.Arrive)
                return false;

            return !inWindow || WithinWindow(result, session, puzzle);
        }

        private static bool WithinWindow(
            TrajectoryResult result, SpaceShapingSession session, PuzzleData puzzle) =>
            result.ShipTime >= puzzle.tmin
            && result.ShipTime <= puzzle.tmax
            && session.EntropySpent <= puzzle.entropyBudget;
    }
}
