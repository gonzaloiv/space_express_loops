using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public readonly struct SpaceZone
    {
        public readonly Vector3 Centre;
        public readonly Vector3 HalfExtents;
        public readonly float Scale;
        public readonly float Falloff;

        public SpaceZone(Vector3 centre, Vector3 halfExtents, float scale, float falloff)
        {
            Centre = centre;
            HalfExtents = halfExtents;
            Scale = scale;
            Falloff = falloff;
        }

        public bool IsActive => Scale > 0f && (HalfExtents.x > 0f || HalfExtents.y > 0f || HalfExtents.z > 0f);

        public float StepDivisor(Vector3 position) =>
            IsActive ? Mathf.Lerp(1f, Scale, InsideFactor(position)) : 1f;

        public float InsideFactor(Vector3 position)
        {
            Vector3 inset = HalfExtents - Abs(position - Centre);
            float margin = Mathf.Min(inset.x, Mathf.Min(inset.y, inset.z));
            if (Falloff <= 0f)
                return margin >= 0f ? 1f : 0f;

            float extent = Mathf.Min(HalfExtents.x, Mathf.Min(HalfExtents.y, HalfExtents.z));
            return Mathf.Clamp01(margin / (Falloff * extent));
        }

        private static Vector3 Abs(Vector3 value) =>
            new(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }
}
