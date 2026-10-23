using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public static class SourceConstraints
    {
        public static void Apply(Vector3[] positions, float[] scales, Vector3[] homes, SourceLimits limits)
        {
            ClampRangeAndScale(positions, scales, homes, limits);
            Separate(positions, scales, limits.SeparationScale);
        }

        private static void ClampRangeAndScale(Vector3[] positions, float[] scales, Vector3[] homes, SourceLimits limits)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 offset = positions[i] - homes[i];
                if (offset.magnitude > limits.MoveRange)
                    positions[i] = homes[i] + offset.normalized * limits.MoveRange;

                scales[i] = Mathf.Clamp(scales[i], limits.MinScale, limits.MaxScale);
            }
        }

        private static void Separate(Vector3[] positions, float[] scales, float separationScale)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                for (int j = i + 1; j < positions.Length; j++)
                {
                    Vector3 delta = positions[j] - positions[i];
                    float distance = delta.magnitude;
                    float minDistance = (scales[i] + scales[j]) * 0.5f * separationScale;
                    if (distance >= minDistance || distance < 0.0001f)
                        continue;

                    Vector3 push = delta.normalized * ((minDistance - distance) * 0.5f);
                    positions[i] -= push;
                    positions[j] += push;
                }
            }
        }
    }
}
