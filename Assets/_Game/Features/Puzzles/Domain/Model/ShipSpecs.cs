using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public static class ShipSpecs
    {
        public static ShipSpec[] From(PuzzleData puzzle) =>
            From(puzzle.ships, puzzle.Origin, puzzle.Destination);

        // A field snapshot carries ships but no PuzzleData; the fallback pair is the puzzle's
        // single-ship run.
        public static ShipSpec[] From(PuzzleShip[] ships, Vector3 origin, Vector3 destination) =>
            ships != null && ships.Length > 0
                ? Convert(ships)
                : Single(origin, destination);

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
