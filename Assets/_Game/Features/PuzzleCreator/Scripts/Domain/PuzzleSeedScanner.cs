using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSeedScanner
    {
        public static PuzzleSeedScanResult Find(
            PuzzleSeed seed, SpaceShapingSettings settings, int attempts)
        {
            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            PuzzleSeedScanResult result = new() { Seed = seed.seed };
            for (int i = 0; i < attempts; i++)
            {
                int candidate = seed.seed + i;
                PuzzlePlanetListAsset planets = Planets(seed, settings, candidate);
                PuzzleData baked = PuzzleFieldBake.Bake(
                    planets, sim, brief, out PuzzleBakeStatus status, out PuzzleZone[] solution);
                Object.DestroyImmediate(planets);
                result.Status = status;
                if (baked == null)
                    continue;

                result.Found = true;
                result.Seed = candidate;
                result.Puzzle = baked;
                result.Solution = solution;
                return result;
            }

            return result;
        }

        private static PuzzlePlanetListAsset Planets(
            PuzzleSeed seed, SpaceShapingSettings settings, int seedValue)
        {
            PuzzleSeed probe = Object.Instantiate(seed);
            probe.seed = seedValue;
            PuzzleShapeAsset shape = PuzzleArtifactFactory.Shape(probe, settings);
            PuzzlePlanetListAsset planets = PuzzleArtifactFactory.Planets(shape);
            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(probe);
            return planets;
        }
    }
}
