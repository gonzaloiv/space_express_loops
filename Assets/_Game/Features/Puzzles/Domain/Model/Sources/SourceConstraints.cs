using DigitalLove.Game.Spatial;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public static class SourceConstraints
    {
        public static void Apply(Vector3[] positions, float[] scales, SourceSpec[] specs, float separationScale)
        {
            ClampScale(scales, specs);
            Separate(positions, scales, specs, separationScale);
        }

        private static void ClampScale(float[] scales, SourceSpec[] specs)
        {
            for (int i = 0; i < scales.Length; i++)
                scales[i] = Mathf.Clamp(scales[i], specs[i].MinScale, specs[i].MaxScale);
        }

        // A fixed source is immovable: a pair it joins pushes only the other body, so a locked sun
        // stays at the centre while the planets it meets move out.
        private static void Separate(
            Vector3[] positions, float[] scales, SourceSpec[] specs, float separationScale)
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

                    Vector3 push = delta.normalized * (minDistance - distance);
                    if (!specs[i].FixedMass)
                        positions[i] -= push * (specs[j].FixedMass ? 1f : 0.5f);
                    if (!specs[j].FixedMass)
                        positions[j] += push * (specs[i].FixedMass ? 1f : 0.5f);
                }
            }
        }
    }
}
