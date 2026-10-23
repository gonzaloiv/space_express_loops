using UnityEngine;

namespace DigitalLove.Game.Sectors
{
    public readonly struct SectorLayout
    {
        public readonly float SectorRadius;
        public readonly float AnchorRadius;
        public readonly Quaternion AnchorRotation;
        public readonly SectorPlanet[] Planets;

        public SectorLayout(
            float sectorRadius,
            float anchorRadius,
            Quaternion anchorRotation,
            SectorPlanet[] planets)
        {
            SectorRadius = sectorRadius;
            AnchorRadius = anchorRadius;
            AnchorRotation = anchorRotation;
            Planets = planets;
        }

        public int PlanetCount => Planets.Length;
    }
}
