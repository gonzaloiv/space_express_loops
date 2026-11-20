using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Bends every lattice node through the field's warp, on Burst.
    [BurstCompile]
    public struct LatticeWarpJob : IJobParallelFor
    {
        public LatticeWarp Warp;
        [ReadOnly] public NativeArray<Vector3> Rest;
        [WriteOnly] public NativeArray<Vector3> Warped;

        public void Execute(int index) => Warped[index] = Warp.Warp(Rest[index]);
    }
}
