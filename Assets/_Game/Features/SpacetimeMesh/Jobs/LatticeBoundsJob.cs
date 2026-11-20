using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    // The mesh's local bounds: every vertex plus the local origin. MinMax holds the min, then the max.
    [BurstCompile]
    public struct LatticeBoundsJob : IJob
    {
        [ReadOnly] public NativeArray<Vector3> Vertices;
        [WriteOnly] public NativeArray<Vector3> MinMax;

        public void Execute()
        {
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;
            for (int i = 0; i < Vertices.Length; i++)
            {
                min = Vector3.Min(min, Vertices[i]);
                max = Vector3.Max(max, Vertices[i]);
            }

            MinMax[0] = min;
            MinMax[1] = max;
        }
    }
}
