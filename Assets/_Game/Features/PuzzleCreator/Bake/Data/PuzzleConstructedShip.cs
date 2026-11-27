using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // One ship of a constructed fleet: the planet pair and the destination's capture radius. Plain,
    // so the construct's steps stay off the runtime data; the verdict builds PuzzleShip from it.
    public sealed class PuzzleConstructedShip
    {
        public int OriginIndex;
        public int DestinationIndex;
        public Vector3 Origin;
        public Vector3 Destination;
        public float CaptureRadius;
    }
}
