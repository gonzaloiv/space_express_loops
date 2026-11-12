using DigitalLove.Game.Spatial;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    [System.Serializable]
    public struct PuzzleZone
    {
        public Vector3 centre;
        public Vector3 halfExtents;
        public float scale;
        public float falloff;

        public float Width => Zone.Width;

        public float Extent => Zone.Extent;

        public SpaceZone ToSpaceZone() => new(centre, halfExtents, scale, falloff);

        public float Margin(Vector3 point) => Zone.Margin(point);

        public bool Contains(Vector3 point) => Zone.Contains(point);

        private SpaceZone Zone => ToSpaceZone();

        public bool Overlaps(PuzzleZone other, float gapFactor) =>
            Zone.Overlaps(other.ToSpaceZone(), gapFactor);
    }
}
