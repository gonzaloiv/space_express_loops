using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBuilder
    {
        private const float FieldRunScale = 0.7f;
        private const float SpreadMin = 0.6f;
        private const float SpreadMax = 0.7f;
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
                (settings.runDistance * FieldRunScale - PlanetRadiusMax(settings)) / SpreadMax);

        private static float Spread(PuzzleSeed seed) =>
            Mathf.Lerp(SpreadMin, SpreadMax, Mathf.InverseLerp(3f, 6f, seed.planetCount));

        private static SectorShapeData BuildShape(
            PuzzleSeed seed, SpaceShapingSettings settings, float sectorRadius) => new()
        {
            kind = Shape(seed),
            radius = new MinMaxFloat(sectorRadius, sectorRadius),
            anchorScale = new MinMaxFloat(1f, 1f),
            anchorRadius = seed.EffectiveSunRadius(settings),
            planetCount = new MinMaxInt { min = seed.planetCount, max = seed.planetCount },
            planetRadius = new MinMaxFloat(PlanetRadiusMin(settings), PlanetRadiusMax(settings)),
            ringCount = new MinMaxInt { min = Rings(seed), max = Rings(seed) },
            armCount = new MinMaxInt { min = 2, max = 3 },
            spread = Spread(seed),
            inclination = settings.fieldThickness,
            tilt = settings.fieldTilt,
            bulge = settings.fieldBulge,
            minSeparation = settings.separationScale * SeparationMargin
        };

        private static SectorShapeKind Shape(PuzzleSeed seed)
        {
            if (seed.fieldShape == PuzzleFieldShape.Ring)
                return SectorShapeKind.Ring;

            if (seed.fieldShape == PuzzleFieldShape.Spiral)
                return SectorShapeKind.Spiral;

            if (seed.fieldShape == PuzzleFieldShape.Cluster)
                return SectorShapeKind.Cluster;

            return (SectorShapeKind)(((seed.seed % 3) + 3) % 3);
        }

        // At least two rings so the bulge always has an inner ring to sit on; a single ring would dome
        // the rim instead of the centre. SectorShape clamps this to the planet count.
        private static int Rings(PuzzleSeed seed) =>
            Mathf.Max(2, (seed.planetCount + PlanetsPerRing - 1) / PlanetsPerRing);
    }
}
