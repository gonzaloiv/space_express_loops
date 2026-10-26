namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBake
    {
        public static void Apply(PuzzleData puzzle, PuzzleSeed seed, SpaceShapingSettings settings)
        {
            PuzzleFieldPlanet[] planets = PuzzleFieldGenerator.Generate(PuzzleFieldBuilder.Build(seed, settings));

            puzzle.distance = PuzzleFieldLayout.RunLength(planets, seed.distance, settings.captureRadius);
            puzzle.sources = PuzzleSourceBridge.ToSources(planets);
        }
    }
}
