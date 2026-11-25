using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Counts planets in a run's block band. A single-ship job measures ship 0's run; a Clear fleet
    // counts only planets that block at least two stored ships' runs (the shared blocker).
    public static class PuzzleRunBlockers
    {
        public static int Count(PuzzleSource[] sources, int sun, PuzzleData puzzle)
        {
            int count = 0;
            for (int i = 0; i < sources.Length; i++)
            {
                if (i == sun || IsEndpoint(sources[i].home, puzzle.origin, puzzle.destination))
                    continue;

                if (Blocks(sources[i], puzzle.origin, puzzle.destination))
                    count++;
            }

            return count;
        }

        public static int SharedCount(PuzzleSource[] sources, int sun, PuzzleData puzzle)
        {
            if (puzzle.ships == null || puzzle.ships.Length < 2)
                return 0;

            int count = 0;
            for (int i = 0; i < sources.Length; i++)
            {
                if (i != sun && SharedByShips(sources[i], puzzle.ships))
                    count++;
            }

            return count;
        }

        private static bool SharedByShips(PuzzleSource source, PuzzleShip[] ships)
        {
            int blocked = 0;
            for (int i = 0; i < ships.Length; i++)
            {
                if (IsEndpoint(source.home, ships[i].origin, ships[i].destination))
                    continue;

                if (Blocks(source, ships[i].origin, ships[i].destination))
                    blocked++;
            }

            return blocked >= 2;
        }

        private static bool Blocks(PuzzleSource source, Vector3 origin, Vector3 destination)
        {
            float radius = source.scale * 0.5f;
            return PuzzleRunGeometry.DistanceToSegment(source.home, origin, destination)
                < radius * PuzzleRunChoice.PlanetBlockBandFactor;
        }

        private static bool IsEndpoint(Vector3 home, Vector3 origin, Vector3 destination) =>
            (home - origin).sqrMagnitude < 0.000001f
            || (home - destination).sqrMagnitude < 0.000001f;
    }
}
