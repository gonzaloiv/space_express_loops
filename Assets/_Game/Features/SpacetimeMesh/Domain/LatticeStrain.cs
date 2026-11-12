using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    // How squeezed or stretched the lattice is at each sample: the warped spacing to its neighbours
    // along its line, against the spacing at rest. +1 is fully squeezed (spacing at squeezeAt × rest or
    // less), -1 fully stretched (stretchAt × rest or more), 0 untouched.
    public static class LatticeStrain
    {
        private const float MinSpacing = 1e-6f;

        public static void Measure(
            Vector3[] rest, Vector3[] warped, float[] strain, int samplesPerLine,
            float squeezeAt, float stretchAt)
        {
            if (samplesPerLine < 2)
                return;

            for (int start = 0; start + samplesPerLine <= warped.Length; start += samplesPerLine)
            {
                int end = start + samplesPerLine - 1;
                for (int i = start; i <= end; i++)
                    strain[i] = At(Spacing(rest, warped, Mathf.Max(start, i - 1), Mathf.Min(end, i + 1)),
                        squeezeAt, stretchAt);
            }
        }

        private static float Spacing(Vector3[] rest, Vector3[] warped, int previous, int next)
        {
            float atRest = Vector3.Distance(rest[previous], rest[next]);
            return atRest > MinSpacing ? Vector3.Distance(warped[previous], warped[next]) / atRest : 1f;
        }

        private static float At(float ratio, float squeezeAt, float stretchAt) =>
            ratio < 1f
                ? Mathf.InverseLerp(1f, squeezeAt, ratio)
                : -Mathf.InverseLerp(1f, stretchAt, ratio);
    }
}
