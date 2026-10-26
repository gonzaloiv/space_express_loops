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
            PuzzleData puzzle, PuzzleSimSettings settings, int samples, int max)
        {
            List<PuzzleSolution> found = new();
            float[] scales = Scales(settings, Mathf.Max(MinSamples, samples));
            for (int i = 0; i < scales.Length && found.Count < max; i++)
            {
                PuzzleZone zone = Zone(puzzle.ZoneSize, scales[i]);
                if (Arrives(puzzle, settings, zone, out float shipTime, out float entropy))
                    found.Add(Solution(zone, shipTime, entropy));
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

        private static PuzzleSolution Solution(PuzzleZone zone, float shipTime, float entropy) => new()
        {
            Zones = new[] { zone },
            ShipTime = shipTime,
            EntropySpent = entropy
        };

        private static PuzzleZone Zone(Vector3 size, float scale) => new()
        {
            centre = Vector3.zero,
            size = size,
            scale = scale,
            falloff = 0f
        };

        private static bool Arrives(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleZone zone, out float shipTime, out float entropy)
        {
            SpaceShapingSession session = new(SpaceShapingSetup.Build(puzzle, settings));
            session.ApplyZone(zone.ToSpaceZone());
            TrajectoryResult result = session.Simulate(
                session.BuildField(), new TrajectoryIntegrator(), new ShipLaunch());
            shipTime = result.ShipTime;
            entropy = session.EntropySpent;
            return result.Outcome == TrajectoryOutcome.Arrive;
        }
    }
}
