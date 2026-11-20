using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // How visible every warped lattice node is, on Burst.
    [BurstCompile]
    public struct LatticeFadeJob : IJobParallelFor
    {
        public ContentFade Fade;
        [ReadOnly] public NativeArray<Vector3> Warped;
        [WriteOnly] public NativeArray<float> Alpha;

        public void Execute(int index) => Alpha[index] = Fade.Alpha(Warped[index]);
    }
}
