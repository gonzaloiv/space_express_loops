using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // One job's route colours and endpoints, resolved once and shared by the line, the planets and
    // the map diorama. Views below the spawner never read the puzzle.
    public sealed class RoutePlan
    {
        public static readonly RoutePlan Empty = new(System.Array.Empty<ShipRoute>());

        private readonly ShipRoute[] routes;

        private RoutePlan(ShipRoute[] routes) => this.routes = routes;

        public int Count => routes.Length;

        public ShipRoute this[int index] => routes[index];

        public Color ColorAt(int index) =>
            index >= 0 && index < routes.Length ? routes[index].Color : Color.white;

        public static RoutePlan Build(PuzzleData puzzle, ColorValue[] palette)
        {
            ShipSpec[] ships = ShipSpecs.From(puzzle);
            ShipRoute[] routes = new ShipRoute[ships.Length];
            int start = RoutePalette.StartIndex(puzzle.id);
            int length = palette != null ? palette.Length : 0;
            for (int i = 0; i < routes.Length; i++)
            {
                Color color = RoutePalette.ColorAt(
                    palette, RoutePalette.Entry(start, i, length));
                routes[i] = new ShipRoute(ships[i].Origin, ships[i].Destination, color);
            }

            return new RoutePlan(routes);
        }
    }
}
