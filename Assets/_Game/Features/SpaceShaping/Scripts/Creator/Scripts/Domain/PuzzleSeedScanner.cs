using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSeedScanner
    {
        public static PuzzleSeedScanResult Find(
            PuzzleSeed seed, SpaceShapingSettings settings, float windowTightness, int variants, int attempts)
        {
            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleSeedScanResult result = new() { Seed = seed.seed };
            for (int i = 0; i < attempts; i++)
            {
                int candidate = seed.seed + i;
                PuzzlePlanetListAsset planets = Planets(seed, settings, candidate);
                PuzzleData[] baked = PuzzleFieldBake.BakeVariants(
                    planets, sim, windowTightness, variants, out PuzzleBakeStatus status);
                if (baked.Length > 0)
                {
                    result.Found = true;
                    result.Seed = candidate;
                    result.Status = status;
                    result.Variants = baked;
                    return result;
                }

                Object.DestroyImmediate(planets);
                result.Status = status;
            }

            return result;
        }

        private static PuzzlePlanetListAsset Planets(
            PuzzleSeed seed, SpaceShapingSettings settings, int seedValue)
        {
            PuzzleSeed probe = ScriptableObject.CreateInstance<PuzzleSeed>();
            probe.seed = seedValue;
            probe.distance = seed.distance;
            probe.planetCount = seed.planetCount;
            PuzzleShapeAsset shape = PuzzleArtifactFactory.Shape(probe, settings);
            PuzzlePlanetListAsset planets = PuzzleArtifactFactory.Planets(shape);
            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(probe);
            return planets;
        }
    }
}
