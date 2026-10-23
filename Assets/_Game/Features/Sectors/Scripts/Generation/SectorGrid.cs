using UnityEngine;

namespace DigitalLove.Game.Sectors
{
    public static class SectorGrid
    {
        public static SectorCell CellAt(Vector3 position, float cellSize)
        {
            float size = Mathf.Max(cellSize, 0.0001f);
            return new SectorCell(
                Mathf.FloorToInt(position.x / size),
                Mathf.FloorToInt(position.y / size),
                Mathf.FloorToInt(position.z / size));
        }

        public static int CellSeed(int worldSeed, SectorCell cell)
        {
            unchecked
            {
                uint hash = Mix((uint)worldSeed ^ (uint)cell.X);
                hash = Mix(hash + (uint)cell.Y);
                hash = Mix(hash ^ (uint)cell.Z);
                return hash == 0u ? 1 : (int)hash;
            }
        }

        private static uint Mix(uint value)
        {
            unchecked
            {
                value ^= value >> 16;
                value *= 0x7feb352du;
                value ^= value >> 15;
                value *= 0x846ca68bu;
                value ^= value >> 16;
                return value;
            }
        }

        public static bool FitsInCell(float sectorRadius, float gap, float cellSize) =>
            sectorRadius * 2f + gap <= cellSize;

        public static bool SectorsOverlap(
            Vector3 centerA,
            float radiusA,
            Vector3 centerB,
            float radiusB,
            float gap)
        {
            float required = radiusA + radiusB + gap;
            return (centerA - centerB).sqrMagnitude < required * required;
        }
    }
}
