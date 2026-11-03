using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public static class ShipSpecs
    {
        public static ShipSpec[] From(PuzzleData puzzle) =>
            puzzle.ships != null && puzzle.ships.Length > 0
                ? Convert(puzzle.ships)
                : Single(puzzle.Origin, puzzle.Destination);

        public static ShipSpec[] Resolve(SpaceShapingSetup setup) =>
            setup.Ships != null && setup.Ships.Length > 0
                ? setup.Ships
                : Single(setup.Origin, setup.Destination);

        private static ShipSpec[] Convert(PuzzleShip[] ships)
        {
            ShipSpec[] specs = new ShipSpec[ships.Length];
            for (int i = 0; i < ships.Length; i++)
            {
                specs[i] = new ShipSpec
                {
                    Origin = ships[i].origin,
                    Destination = ships[i].destination,
                    CaptureRadius = ships[i].captureRadius
                };
            }

            return specs;
        }

        private static ShipSpec[] Single(Vector3 origin, Vector3 destination) =>
            new[] { new ShipSpec { Origin = origin, Destination = destination } };
    }
}
