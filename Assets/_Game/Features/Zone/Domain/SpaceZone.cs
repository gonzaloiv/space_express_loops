using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    public readonly struct SpaceZone
    {
        private const float MinEffect = 0.1f;

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

        public float Extent => Mathf.Min(HalfExtents.x, Mathf.Min(HalfExtents.y, HalfExtents.z));

        public float Width => HalfExtents.x * 2f;

        public bool Contains(Vector3 position) => Margin(position) >= 0f;

        public bool Overlaps(SpaceZone other, float gapFactor)
        {
            float gap = Mathf.Max(gapFactor, 0f) * Mathf.Min(Width, other.Width);
            Vector3 delta = Abs(Centre - other.Centre);
            Vector3 extent = HalfExtents + other.HalfExtents;
            return delta.x < extent.x + gap
                && delta.y < extent.y + gap
                && delta.z < extent.z + gap;
        }

        public static bool BoxContains(Vector3 point, Vector3 centre, Vector3 halfExtents) =>
            Mathf.Abs(point.x - centre.x) <= halfExtents.x
            && Mathf.Abs(point.y - centre.y) <= halfExtents.y
            && Mathf.Abs(point.z - centre.z) <= halfExtents.z;

        public float Margin(Vector3 position)
        {
            Vector3 inset = HalfExtents - Abs(position - Centre);
            return Mathf.Min(inset.x, Mathf.Min(inset.y, inset.z));
        }

        public bool Influences(Vector3 position) =>
            Margin(position) + Mathf.Max(Falloff, 0f) * Extent > 0f;

        public bool SameRegion(SpaceZone other) =>
            (Centre - other.Centre).sqrMagnitude < 0.000001f
            && (HalfExtents - other.HalfExtents).sqrMagnitude < 0.000001f;

        public SpaceZone Effective(float gain)
        {
            float applied = gain > 0f ? gain : 1f;
            float effect = Mathf.Max(1f + (Scale - 1f) * applied, MinEffect);
            return new SpaceZone(Centre, HalfExtents, effect, Falloff);
        }

        public void ApplyTo(Vector3[] positions, float[] scales, SourceSpec[] specs)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                if (specs[i].FixedMass)
                    continue;

                float factor = Mathf.Lerp(1f, Scale, InsideFactor(positions[i]));
                positions[i] = Centre + (positions[i] - Centre) * factor;
                scales[i] *= factor;
                specs[i] = specs[i].Scaled(Centre, factor);
            }
        }

        public float StepDivisor(Vector3 position) =>
            IsActive ? Mathf.Lerp(1f, Scale, InsideFactor(position)) : 1f;

        public float InsideFactor(Vector3 position)
        {
            float margin = Margin(position);
            if (Falloff <= 0f)
                return margin >= 0f ? 1f : 0f;

            return Mathf.Clamp01(margin / (Falloff * Extent));
        }

        private static Vector3 Abs(Vector3 value) =>
            new(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }
}
