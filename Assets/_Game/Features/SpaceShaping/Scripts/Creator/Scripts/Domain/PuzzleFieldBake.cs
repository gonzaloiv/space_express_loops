namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBake
    {
        public static void Apply(PuzzleData puzzle, PuzzlePlanetListAsset planets)
        {
            puzzle.distance = planets.distance;
            puzzle.sources = PuzzleSourceBridge.ToSources(planets.planets);
        }
    }
}
