using DigitalLove.Game.SpaceShaping;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The seed values the field shape needs, without the ScriptableObject, so the constructor's
    // steps stay plain data.
    public readonly struct PuzzleSeedFields
    {
        public readonly int Seed;
        public readonly int PlanetCount;
        public readonly PuzzleFieldShape FieldShape;
        public readonly float SunRadius;

        public PuzzleSeedFields(
            int seed, int planetCount, PuzzleFieldShape fieldShape, float sunRadius)
        {
            Seed = seed;
            PlanetCount = planetCount;
            FieldShape = fieldShape;
            SunRadius = sunRadius;
        }

        public static PuzzleSeedFields From(PuzzleSeed seed, SpaceShapingSettings settings) =>
            new(seed.seed, seed.planetCount, seed.fieldShape, seed.EffectiveSunRadius(settings));

        public PuzzleSeedFields WithSeed(int seed) =>
            new(seed, PlanetCount, FieldShape, SunRadius);
    }
}
