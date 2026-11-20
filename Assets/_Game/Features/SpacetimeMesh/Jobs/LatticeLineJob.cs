using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    // One lattice line: smooths its warp, then measures its strain. Lines never share samples, so each
    // runs on its own worker.
    [BurstCompile]
    public struct LatticeLineJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> Rest;
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> Warped;
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> Scratch;
        [NativeDisableParallelForRestriction] public NativeArray<float> Strains;
        public int SamplesPerLine;
        public int SmoothPasses;
        public float SqueezeAt;
        public float StretchAt;

        public void Execute(int line)
        {
            int start = line * SamplesPerLine;
            LatticeLineSmoothing.SmoothLine(Rest, Warped, Scratch, start, SamplesPerLine, SmoothPasses);
            LatticeStrain.MeasureLine(Rest, Warped, Strains, start, SamplesPerLine, SqueezeAt, StretchAt);
        }
    }
}
