using UnityEngine;

namespace DigitalLove.Game.Sectors
{
    public readonly struct UniverseBounds
    {
        public readonly Vector3 Origin;
        public readonly float FieldRadius;

        public UniverseBounds(Vector3 origin, float fieldRadius)
        {
            Origin = origin;
            FieldRadius = Mathf.Max(0f, fieldRadius);
        }

        public Vector3 ClampFocus(Vector3 focus, float viewRadius)
        {
            float limit = Mathf.Max(0f, FieldRadius - Mathf.Max(0f, viewRadius));
            Vector3 local = focus - Origin;
            return Origin + new Vector3(
                Mathf.Clamp(local.x, -limit, limit),
                Mathf.Clamp(local.y, -limit, limit),
                Mathf.Clamp(local.z, -limit, limit));
        }

        public bool Contains(Vector3 point) => (point - Origin).magnitude <= FieldRadius;
    }
}
