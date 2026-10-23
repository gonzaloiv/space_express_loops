using UnityEngine;

namespace DigitalLove.Game.Sectors
{
    public readonly struct SectorCell
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public SectorCell(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 Center(float cellSize) => new(X * cellSize, Y * cellSize, Z * cellSize);
    }
}
