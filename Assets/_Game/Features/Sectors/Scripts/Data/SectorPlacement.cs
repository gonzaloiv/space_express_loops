using UnityEngine;

namespace DigitalLove.Game.Sectors
{
    public readonly struct SectorPlacement
    {
        public readonly SectorCell Cell;
        public readonly Vector3 Center;
        public readonly SectorLayout Layout;

        public SectorPlacement(SectorCell cell, Vector3 center, SectorLayout layout)
        {
            Cell = cell;
            Center = center;
            Layout = layout;
        }

        public int PlanetCount => Layout.PlanetCount;
    }
}
