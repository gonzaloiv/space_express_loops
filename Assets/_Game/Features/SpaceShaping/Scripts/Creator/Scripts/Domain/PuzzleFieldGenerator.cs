using System.Collections.Generic;
using DigitalLove.Game.Sectors;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldGenerator
    {
        public static PuzzleFieldPlanet[] Generate(SectorFieldData field)
        {
            SectorPlacement[] placements = SectorField.Generate(field);
            List<PuzzleFieldPlanet> planets = new();
            for (int p = 0; p < placements.Length; p++)
                AddSector(placements[p], planets);

            return planets.ToArray();
        }

        private static void AddSector(SectorPlacement placement, List<PuzzleFieldPlanet> planets)
        {
            SectorLayout layout = placement.Layout;
            for (int i = 0; i < layout.PlanetCount; i++)
            {
                SectorPlanet planet = layout.Planets[i];
                planets.Add(new PuzzleFieldPlanet(placement.Center + planet.Position, planet.Radius));
            }
        }
    }
}
