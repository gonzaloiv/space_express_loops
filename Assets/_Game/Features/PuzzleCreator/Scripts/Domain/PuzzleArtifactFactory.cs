using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleArtifactFactory
    {
        public static PuzzleShapeAsset Shape(PuzzleSeed seed, SpaceShapingSettings settings)
        {
            PuzzleShapeAsset asset = ScriptableObject.CreateInstance<PuzzleShapeAsset>();
            asset.seed = seed.seed;
            asset.distance = seed.distance;
            asset.clearance = seed.corridorClearance;
            asset.shape = PuzzleFieldBuilder.Build(seed, settings);
            return asset;
        }

        public static PuzzlePlanetListAsset Planets(PuzzleShapeAsset shape)
        {
            PuzzlePlanetListAsset asset = ScriptableObject.CreateInstance<PuzzlePlanetListAsset>();
            asset.distance = shape.distance;
            asset.clearance = shape.clearance;
            asset.planets = PuzzleFieldGenerator.Generate(shape.shape, shape.seed);
            return asset;
        }
    }
}
