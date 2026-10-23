using UnityEngine;

namespace DigitalLove.Game.Sectors
{
    public readonly struct UniverseFocus
    {
        public readonly Vector3 Position;
        public readonly float ViewRadius;

        public UniverseFocus(Vector3 position, float viewRadius)
        {
            Position = position;
            ViewRadius = Mathf.Max(0f, viewRadius);
        }

        public UniverseFocus Moved(Vector3 delta, UniverseBounds bounds) =>
            new(bounds.ClampFocus(Position + delta, ViewRadius), ViewRadius);
    }
}
