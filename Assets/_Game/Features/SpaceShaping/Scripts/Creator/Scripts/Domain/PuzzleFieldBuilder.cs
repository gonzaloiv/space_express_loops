using DigitalLove.Game.Sectors;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBuilder
    {
        private const float FieldMargin = 0.1f;
        private const float Spread = 0.7f;
        private const float Thickness = 0.3f;
        private const float PlanetRadiusFraction = 0.06f;
        private const float PlanetRadiusSpread = 0.3f;
        private const int PlanetsPerRing = 6;
        private const float SeparationMargin = 1.1f;

        public static SectorShapeData Build(PuzzleSeed seed, SpaceShapingSettings settings)
        {
            float sectorRadius = SectorRadius(seed, settings);
            return BuildShape(seed, settings, sectorRadius);
        }

        private static float PlanetRadius(PuzzleSeed seed) => seed.distance * PlanetRadiusFraction;

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
            minSeparation = settings.separationScale * SeparationMargin
        };

        private static SectorShapeKind Shape(PuzzleSeed seed) =>
            (SectorShapeKind)(((seed.seed % 3) + 3) % 3);

        private static int Rings(PuzzleSeed seed) =>
            Mathf.Max(1, (seed.planetCount + PlanetsPerRing - 1) / PlanetsPerRing);
    }
}
