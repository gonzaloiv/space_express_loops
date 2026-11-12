using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // One ship's presentation route: its two endpoints and the colour that draws it.
    public readonly struct ShipRoute
    {
        public readonly Vector3 Origin;
        public readonly Vector3 Destination;
        public readonly Color Color;

        public ShipRoute(Vector3 origin, Vector3 destination, Color color)
        {
            Origin = origin;
            Destination = destination;
            Color = color;
        }
    }
}
