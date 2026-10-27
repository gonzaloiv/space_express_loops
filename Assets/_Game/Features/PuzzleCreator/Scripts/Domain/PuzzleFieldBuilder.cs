using DigitalLove.Game.Sectors;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBuilder
    {
        private const float FieldRunScale = 0.7f;
        private const float Spread = 0.7f;
        private const float Thickness = 0.08f;
        private const int PlanetsPerRing = 6;
        private const float SeparationMargin = 1.1f;
        private const float MinSectorRadius = 0.1f;

        public static SectorShapeData Build(PuzzleSeed seed, SpaceShapingSettings settings)
        {
            float sectorRadius = SectorRadius(settings);
            return BuildShape(seed, settings, sectorRadius);
        }

        private static float PlanetRadiusMin(SpaceShapingSettings settings) => settings.planetRadiusMin;

        private static float PlanetRadiusMax(SpaceShapingSettings settings) => settings.planetRadiusMax;

        private static float SectorRadius(SpaceShapingSettings settings) =>
            Mathf.Max(
                MinSectorRadius,
                (settings.runDistance * FieldRunScale - PlanetRadiusMax(settings)) / Spread);

        private static SectorShapeData BuildShape(
            PuzzleSeed seed, SpaceShapingSettings settings, float sectorRadius) => new()
        {
            kind = Shape(seed),
            radius = new MinMaxFloat(sectorRadius, sectorRadius),
            anchorScale = new MinMaxFloat(1f, 1f),
            planetCount = new MinMaxInt { min = seed.planetCount, max = seed.planetCount },
            planetRadius = new MinMaxFloat(PlanetRadiusMin(settings), PlanetRadiusMax(settings)),
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
