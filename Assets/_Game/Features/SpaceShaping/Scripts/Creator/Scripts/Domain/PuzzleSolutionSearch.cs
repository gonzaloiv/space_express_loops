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
            solution = default;
            Vector3 size = Vector3.one * puzzle.distance;
            int count = Mathf.Max(MinSamples, samples);
            for (int i = 0; i < count; i++)
            {
                PuzzleZone zone = Candidate(size, settings, i, count);
                if (Arrives(puzzle, settings, zone, out float shipTime, out float entropy))
                {
                    solution = new PuzzleSolution
                    {
                        Zones = new[] { zone },
                        ShipTime = shipTime,
                        EntropySpent = entropy
                    };
                    return true;
                }
            }

            return false;
        }

        private static PuzzleZone Candidate(Vector3 size, PuzzleSimSettings settings, int index, int count)
        {
            float t = index / (float)(count - 1);
            return new PuzzleZone
            {
                centre = Vector3.zero,
                size = size,
                scale = Mathf.Lerp(settings.MinScaleFactor, settings.MaxScaleFactor, t),
                falloff = 0f
            };
        }

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
