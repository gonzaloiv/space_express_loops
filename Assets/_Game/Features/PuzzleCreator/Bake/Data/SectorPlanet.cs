using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public readonly struct SectorPlanet
    {
        public readonly Vector3 Position;
        public readonly float Radius;

        public SectorPlanet(Vector3 position, float radius)
        {
            Position = position;
            Radius = radius;
        }
    }
}
