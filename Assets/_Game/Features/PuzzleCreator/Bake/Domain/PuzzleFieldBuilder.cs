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

        public static SectorShapeData Build(PuzzleSeed seed, SpaceShapingSettings settings) =>
            Build(PuzzleSeedFields.From(seed, settings), PuzzleFieldSettings.From(settings));

        public static SectorShapeData Build(PuzzleSeedFields seed, PuzzleFieldSettings settings)
        {
            float sectorRadius = SectorRadius(settings);
            return BuildShape(seed, settings, sectorRadius);
        }

        private static float SectorRadius(PuzzleFieldSettings settings) =>
            Mathf.Max(
                MinSectorRadius,
                (settings.RunDistance * FieldRunScale - settings.PlanetRadiusMax) / SpreadMax);

        private static float Spread(PuzzleSeedFields seed) =>
            Mathf.Lerp(SpreadMin, SpreadMax, Mathf.InverseLerp(3f, 6f, seed.PlanetCount));

        private static SectorShapeData BuildShape(
            PuzzleSeedFields seed, PuzzleFieldSettings settings, float sectorRadius) => new()
        {
            kind = Shape(seed),
            radius = new MinMaxFloat(sectorRadius, sectorRadius),
            anchorScale = new MinMaxFloat(1f, 1f),
            anchorRadius = seed.SunRadius,
            planetCount = new MinMaxInt { min = seed.PlanetCount, max = seed.PlanetCount },
            planetRadius = new MinMaxFloat(
                settings.PlanetRadiusMin, settings.PlanetRadiusMax),
            ringCount = new MinMaxInt { min = Rings(seed), max = Rings(seed) },
            armCount = new MinMaxInt { min = 2, max = 3 },
            spread = Spread(seed),
            inclination = settings.FieldThickness,
            tilt = settings.FieldTilt,
            bulge = settings.FieldBulge,
            minSeparation = settings.SeparationScale * SeparationMargin
        };

        private static SectorShapeKind Shape(PuzzleSeedFields seed)
        {
            if (seed.FieldShape == PuzzleFieldShape.Ring)
                return SectorShapeKind.Ring;

            if (seed.FieldShape == PuzzleFieldShape.Spiral)
                return SectorShapeKind.Spiral;

            if (seed.FieldShape == PuzzleFieldShape.Cluster)
                return SectorShapeKind.Cluster;

            return (SectorShapeKind)(((seed.Seed % 3) + 3) % 3);
        }

        // At least two rings so the bulge always has an inner ring to sit on; a single ring would dome
        // the rim instead of the centre. SectorShape clamps this to the planet count.
        private static int Rings(PuzzleSeedFields seed) =>
            Mathf.Max(2, (seed.PlanetCount + PlanetsPerRing - 1) / PlanetsPerRing);
    }
}
