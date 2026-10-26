using DigitalLove.Game.Sectors;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBuilder
    {
        private const float FieldGap = 0.5f;
        private const float FieldMargin = 1f;
        private const float Spread = 0.7f;
        private const float Thickness = 0.3f;
        private const float PlanetRadiusFraction = 0.05f;
        private const float PlanetRadiusSpread = 0.3f;
        private const int PlanetsPerRing = 6;

        public static SectorFieldData Build(PuzzleSeed seed, SpaceShapingSettings settings)
        {
            float sectorRadius = SectorRadius(seed, settings);
            float fieldRadius = sectorRadius + PlanetRadiusMax(seed) + FieldMargin;
            return new SectorFieldData
            {
                worldSeed = seed.seed,
                cellSize = fieldRadius * 2f,
                fieldRadius = fieldRadius,
                density = 1f,
                gap = FieldGap,
                layers = 1,
                softSectorBudget = 1,
                softPlanetBudget = seed.planetCount,
                shape = BuildShape(seed, settings, sectorRadius)
            };
        }

        public static float PlanetRadius(PuzzleSeed seed) => seed.distance * PlanetRadiusFraction;

        private static float PlanetRadiusMax(PuzzleSeed seed) =>
            PlanetRadius(seed) * (1f + PlanetRadiusSpread);

        private static float SectorRadius(PuzzleSeed seed, SpaceShapingSettings settings) =>
            Mathf.Max(
                0.1f,
                (seed.distance * 0.5f - PlanetRadiusMax(seed) - settings.captureRadius - FieldMargin) / Spread);

        private static SectorShapeData BuildShape(
            PuzzleSeed seed, SpaceShapingSettings settings, float sectorRadius) => new()
        {
            kind = Shape(seed),
            radius = new MinMaxFloat(sectorRadius * 0.6f, sectorRadius),
            anchorScale = new MinMaxFloat(1f, 1f),
            planetCount = new MinMaxInt { min = seed.planetCount, max = seed.planetCount },
            planetRadius = new MinMaxFloat(
                PlanetRadius(seed) * (1f - PlanetRadiusSpread), PlanetRadiusMax(seed)),
            ringCount = new MinMaxInt { min = Rings(seed), max = Rings(seed) },
            armCount = new MinMaxInt { min = 2, max = 3 },
            spread = Spread,
            inclination = Thickness,
            minSeparation = settings.separationScale
        };

        private static SectorShapeKind Shape(PuzzleSeed seed) =>
            (SectorShapeKind)(((seed.seed % 3) + 3) % 3);

        private static int Rings(PuzzleSeed seed) =>
            Mathf.Max(1, (seed.planetCount + PlanetsPerRing - 1) / PlanetsPerRing);
    }
}
