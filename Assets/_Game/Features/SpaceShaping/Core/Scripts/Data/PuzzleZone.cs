using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    [System.Serializable]
    public struct PuzzleZone
    {
        public Vector3 centre;
        public Vector3 halfExtents;
        public float scale;
        public float falloff;

        public float Width => halfExtents.x * 2f;

        public float Extent => Mathf.Min(halfExtents.x, Mathf.Min(halfExtents.y, halfExtents.z));

        public SpaceZone ToSpaceZone() => new(centre, halfExtents, scale, falloff);

        public float Margin(Vector3 point)
        {
            Vector3 inset = halfExtents - Abs(point - centre);
            return Mathf.Min(inset.x, Mathf.Min(inset.y, inset.z));
        }

        public bool Influences(Vector3 point) =>
            Margin(point) + Mathf.Max(falloff, 0f) * Extent > 0f;

        public bool Contains(Vector3 point)
        {
            Vector3 offset = Abs(point - centre);
            return offset.x <= halfExtents.x
                && offset.y <= halfExtents.y
                && offset.z <= halfExtents.z;
        }

        public bool Overlaps(PuzzleZone other, float gapFactor)
        {
            float gap = Mathf.Max(gapFactor, 0f) * Mathf.Min(Width, other.Width);
            Vector3 delta = Abs(centre - other.centre);
            Vector3 extent = halfExtents + other.halfExtents;
            return delta.x < extent.x + gap
                && delta.y < extent.y + gap
                && delta.z < extent.z + gap;
        }

        private static Vector3 Abs(Vector3 value) =>
            new(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }
}
