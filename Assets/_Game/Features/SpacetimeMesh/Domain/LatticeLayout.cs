using Unity.Collections;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    // How the baked lattice mesh is laid out: three axes of gridCount² lines, each line samplesPerLine
    // rings of `sides` consecutive vertices. Decodes every ring's node and every vertex's ring offset.
    public readonly struct LatticeLayout
    {
        private readonly int gridCount;
        private readonly int sides;
        private readonly int samplesPerLine;

        public LatticeLayout(int gridCount, int sides, int samplesPerLine)
        {
            this.gridCount = gridCount;
            this.sides = sides;
            this.samplesPerLine = samplesPerLine;
        }

        public void Fill(NativeArray<Vector3> ringFractions, NativeArray<Vector3> ringOffsets)
        {
            for (int v = 0; v < ringOffsets.Length; v++)
            {
                LatticeSample sample = Decode(v);
                ringOffsets[v] = LatticeGrid.RingOffset(sample.Axis, sample.Side, sides);
                if (sample.Side == 0)
                    ringFractions[v / sides] = LatticeGrid.Node(
                        sample.Axis, sample.NodeIndex, sample.I, sample.J, gridCount,
                        Vector3.zero, Vector3.one);
            }
        }

        private LatticeSample Decode(int vertex)
        {
            int perLine = samplesPerLine * sides;
            int perAxisVertices = gridCount * gridCount * perLine;
            int axis = vertex / perAxisVertices;
            int rem = vertex % perAxisVertices;
            int line = rem / perLine;
            int rem2 = rem % perLine;
            float nodeIndex = (float)(rem2 / sides) / (samplesPerLine - 1) * (gridCount - 1);
            return new LatticeSample(
                axis, line / gridCount, line % gridCount, rem2 % sides, nodeIndex);
        }
    }
}
