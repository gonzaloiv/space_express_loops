using UnityEngine;

namespace DigitalLove.Game.Sectors
{
    public readonly struct SectorSelection
    {
        public readonly SectorPlacement Placement;
        public readonly int PlanetIndex;
        public readonly Vector3 Position;
        public readonly float Radius;
        public readonly int VariantSeed;

        public SectorSelection(
            SectorPlacement placement,
            int planetIndex,
            Vector3 position,
            float radius,
            int variantSeed)
        {
            Placement = placement;
            PlanetIndex = planetIndex;
            Position = position;
            Radius = radius;
            VariantSeed = variantSeed;
        }

        public bool IsAnchor => PlanetIndex < 0;

        public SectorCell Cell => Placement.Cell;
    }
}
