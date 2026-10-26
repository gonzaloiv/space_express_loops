using DigitalLove.Game.Sectors;
using DigitalLove.Global;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBuilder
    {
        private const float FieldGap = 0.5f;
        private const float FieldMargin = 1f;
        private const float Spread = 0.7f;
        private const float Thickness = 0.3f;
        private const float PlanetRadiusFraction = 0.05f;

        public static SectorFieldData Build(PuzzleSeed seed, SpaceShapingSettings settings)
        {
            float radius = FieldRadius(seed);
            return new SectorFieldData
            {
                worldSeed = seed.seed,
                cellSize = radius * 2f,
                fieldRadius = radius,
                density = 1f,
                gap = FieldGap,
                layers = 1,
                softSectorBudget = 1,
                softPlanetBudget = seed.planetCount,
                shape = BuildShape(seed, settings, radius)
            };
        }

        public static float PlanetRadius(PuzzleSeed seed) => seed.distance * PlanetRadiusFraction;

        private static float FieldRadius(PuzzleSeed seed) =>
            seed.distance * 0.5f + PlanetRadius(seed) * 2f + FieldMargin;

        private static SectorShapeData BuildShape(
            PuzzleSeed seed, SpaceShapingSettings settings, float fieldRadius) => new()
        {
            kind = Shape(seed),
            radius = new MinMaxFloat(fieldRadius * 0.5f, fieldRadius * 0.75f),
            anchorScale = new MinMaxFloat(1f, 1f),
            planetCount = new MinMaxInt { min = seed.planetCount, max = seed.planetCount },
            planetRadius = new MinMaxFloat(PlanetRadius(seed), PlanetRadius(seed)),
            ringCount = new MinMaxInt { min = 1, max = 1 },
            armCount = new MinMaxInt { min = 2, max = 3 },
            spread = Spread,
            inclination = Thickness,
            minSeparation = settings.separationScale
        };

        private static SectorShapeKind Shape(PuzzleSeed seed) =>
            (SectorShapeKind)(((seed.seed % 3) + 3) % 3);
    }
}
