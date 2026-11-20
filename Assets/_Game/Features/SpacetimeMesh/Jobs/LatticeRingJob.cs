using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    // Writes one tube ring: its `sides` vertices share the warped node, so the ring is placed, sized by
    // the strain, and coloured by the strain and the fade once.
    [BurstCompile]
    public struct LatticeRingJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> Warped;
        [ReadOnly] public NativeArray<float> Strains;
        [ReadOnly] public NativeArray<float> Alpha;
        [ReadOnly] public NativeArray<Vector3> RingOffsets;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<Vector3> Vertices;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<Color> Colors;
        public Matrix4x4 ParentToLocal;
        public LatticeStrainLook Look;
        public float Radius;
        public int Sides;

        public void Execute(int ring)
        {
            Vector3 centre = ParentToLocal.MultiplyPoint3x4(Warped[ring]);
            float strain = Strains[ring];
            Color color = Look.VertexColor(Alpha[ring], strain);
            float ringRadius = Look.Radius(Radius, strain);
            int first = ring * Sides;
            for (int v = first; v < first + Sides; v++)
            {
                Vertices[v] = centre + RingOffsets[v] * ringRadius;
                Colors[v] = color;
            }
        }
    }
}
