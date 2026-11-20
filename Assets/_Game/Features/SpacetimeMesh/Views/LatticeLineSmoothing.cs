using Unity.Collections;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    // Softens the lattice's spikes without more points: along each line, every sample's warp
    // displacement is averaged with its neighbours' (a 1-2-1 kernel, repeated). Line ends keep their
    // own displacement, so the lattice still meets the field's frame.
    public static class LatticeLineSmoothing
    {
        public static void SmoothLine(
            NativeArray<Vector3> rest, NativeArray<Vector3> warped, NativeArray<Vector3> scratch,
            int start, int count, int passes)
        {
            if (passes <= 0 || count < 3)
                return;

            int end = start + count - 1;
            for (int pass = 0; pass < passes; pass++)
            {
                scratch[start] = warped[start];
                scratch[end] = warped[end];
                for (int i = start + 1; i < end; i++)
                {
                    Vector3 previous = warped[i - 1] - rest[i - 1];
                    Vector3 current = warped[i] - rest[i];
                    Vector3 next = warped[i + 1] - rest[i + 1];
                    scratch[i] = rest[i] + (previous + 2f * current + next) * 0.25f;
                }

                for (int i = start; i <= end; i++)
                    warped[i] = scratch[i];
            }
        }
    }
}
