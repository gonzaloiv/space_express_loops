using DigitalLove.Game.Sectors;
using DigitalLove.Global;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldGenerator
    {
        public static PuzzleFieldPlanet[] Generate(SectorShapeData shape, int seed)
        {
            SectorLayout layout = SectorShape.Generate(shape, new SeededRandom(seed));
            PuzzleFieldPlanet[] planets = new PuzzleFieldPlanet[layout.PlanetCount];
            for (int i = 0; i < planets.Length; i++)
                planets[i] = new PuzzleFieldPlanet(layout.Planets[i].Position, layout.Planets[i].Radius);

            return planets;
        }
    }
}
