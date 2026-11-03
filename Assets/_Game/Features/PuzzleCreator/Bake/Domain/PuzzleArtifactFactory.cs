using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleArtifactFactory
    {
        public static PuzzleShapeAsset Shape(PuzzleSeed seed, SpaceShapingSettings settings)
        {
            PuzzleShapeAsset asset = ScriptableObject.CreateInstance<PuzzleShapeAsset>();
            asset.seed = seed.seed;
            asset.shape = PuzzleFieldBuilder.Build(seed, settings);
            return asset;
        }

        public static PuzzlePlanetListAsset Planets(PuzzleShapeAsset shape)
        {
            PuzzlePlanetListAsset asset = ScriptableObject.CreateInstance<PuzzlePlanetListAsset>();
            asset.planets = PuzzleFieldGenerator.Generate(shape.shape, shape.seed);
            PlanetNames.Instance.Assign(asset.planets);
            return asset;
        }

        // Planets for a probe seed: the seed clone, shape, and planet list are all transient.
        public static PuzzlePlanetListAsset PlanetsFor(
            PuzzleSeed seed, SpaceShapingSettings settings, int seedValue)
        {
            PuzzleSeed probe = Object.Instantiate(seed);
            probe.seed = seedValue;
            PuzzleShapeAsset shape = Shape(probe, settings);
            PuzzlePlanetListAsset planets = Planets(shape);
            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(probe);
            return planets;
        }
    }
}
