using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Every ship's two endpoints, derived from the authored ships; single-ship data falls back to
    // the puzzle's origin and destination. Read-only and allocation-free for hot bake loops.
    public readonly struct PuzzleEndpoints
    {
        private readonly PuzzleShip[] ships;
        private readonly Vector3 origin;
        private readonly Vector3 destination;

        public PuzzleEndpoints(PuzzleShip[] ships, Vector3 origin, Vector3 destination)
        {
            this.ships = ships;
            this.origin = origin;
            this.destination = destination;
        }

        public int Count => ships != null && ships.Length > 0 ? ships.Length * 2 : 2;

        public Vector3 this[int index]
        {
            get
            {
                if (ships == null || ships.Length == 0)
                    return index == 0 ? origin : destination;

                int ship = index / 2;
                return index % 2 == 0 ? ships[ship].origin : ships[ship].destination;
            }
        }

        public Vector3[] ToArray()
        {
            Vector3[] points = new Vector3[Count];
            for (int i = 0; i < points.Length; i++)
                points[i] = this[i];

            return points;
        }
    }
}
