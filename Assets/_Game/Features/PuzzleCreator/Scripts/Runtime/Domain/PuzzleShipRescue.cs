using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public readonly struct PuzzleShipRescue
    {
        public readonly Vector3 Origin;
        public readonly Vector3 Destination;
        public readonly float Margin;

        public PuzzleShipRescue(Vector3 origin, Vector3 destination, float margin)
        {
            Origin = origin;
            Destination = destination;
            Margin = margin;
        }
    }
}
