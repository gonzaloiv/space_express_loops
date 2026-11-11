using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    // Softens the lattice's spikes without more points: along each line, every sample's warp
    // displacement is averaged with its neighbours' (a 1-2-1 kernel, repeated). Line ends keep their
    // own displacement, so the lattice still meets the field's frame.
    public static class LatticeLineSmoothing
    {
        public static void Smooth(
            Vector3[] nodes, Vector3[] warped, Vector3[] scratch, int samplesPerLine, int passes)
        {
            if (passes <= 0 || samplesPerLine < 3)
                return;

            for (int pass = 0; pass < passes; pass++)
            {
                for (int start = 0; start + samplesPerLine <= warped.Length; start += samplesPerLine)
                    SmoothLine(nodes, warped, scratch, start, samplesPerLine);

                for (int i = 0; i < warped.Length; i++)
                    warped[i] = scratch[i];
            }
        }

        private static void SmoothLine(
            Vector3[] nodes, Vector3[] warped, Vector3[] scratch, int start, int count)
        {
            int end = start + count - 1;
            scratch[start] = warped[start];
            scratch[end] = warped[end];
            for (int i = start + 1; i < end; i++)
            {
                Vector3 previous = warped[i - 1] - nodes[i - 1];
                Vector3 current = warped[i] - nodes[i];
                Vector3 next = warped[i + 1] - nodes[i + 1];
                scratch[i] = nodes[i] + (previous + 2f * current + next) * 0.25f;
            }
        }
    }
}
