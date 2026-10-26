namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBake
    {
        public static void Apply(PuzzleData puzzle, PuzzleSeed seed, SpaceShapingSettings settings)
        {
            puzzle.distance = seed.distance;
            puzzle.sources = PuzzleSourceBridge.ToSources(
                PuzzleFieldGenerator.Generate(PuzzleFieldBuilder.Build(seed, settings)));
        }
    }
}
