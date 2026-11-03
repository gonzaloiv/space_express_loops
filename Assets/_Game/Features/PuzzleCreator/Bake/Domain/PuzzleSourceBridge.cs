namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSourceBridge
    {
        public static PuzzleSource[] ToSources(PuzzleFieldPlanet[] planets)
        {
            PuzzleSource[] sources = new PuzzleSource[planets.Length];
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i] = new PuzzleSource
                {
                    home = planets[i].position,
                    scale = planets[i].radius * 2f,
                    name = planets[i].name
                };
            }

            return sources;
        }
    }
}
