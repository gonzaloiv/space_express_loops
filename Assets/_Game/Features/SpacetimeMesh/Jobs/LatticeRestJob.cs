using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    // Places every ring's node at rest: its fraction of the unit lattice, sized to the field's half and
    // carried into the field's (parent) space, where the warp works.
    [BurstCompile]
    public struct LatticeRestJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> Fractions;
        [WriteOnly] public NativeArray<Vector3> Rest;
        public Matrix4x4 LocalToParent;
        public float Half;

        public void Execute(int ring)
        {
            Vector3 node = (Fractions[ring] * 2f - Vector3.one) * Half;
            Rest[ring] = LocalToParent.MultiplyPoint3x4(node);
        }
    }
}
