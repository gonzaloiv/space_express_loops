using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpacetimeMeshView : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private Vector3 gridMin = new(-6f, -4f, -3f);
        [SerializeField] private Vector3 gridMax = new(6f, 4f, 3f);
        [SerializeField] private int gridCount = 4;
        [SerializeField] private int samplesPerLine = 16;
        [SerializeField] private int sides = 6;
        [SerializeField] private float radius = 0.05f;
        [SerializeField] private float depthScale = 1.5f;
        [SerializeField] private float maxDisplacement = 0.8f;

        private Mesh mesh;
        private Vector3[] vertices;

        public void Render(SpacetimeField field, IReadOnlyList<SpaceZone> zones)
        {
            EnsureMesh();

            int perAxis = gridCount * gridCount;
            int perLine = samplesPerLine * sides;
            int perAxisVertices = perAxis * perLine;

            for (int v = 0; v < vertices.Length; v++)
            {
                int axis = v / perAxisVertices;
                int rem = v % perAxisVertices;
                int line = rem / perLine;
                int rem2 = rem % perLine;
                int sample = rem2 / sides;
                int side = rem2 % sides;

                int i = line / gridCount;
                int j = line % gridCount;
                float nodeIndex = (float)sample / (samplesPerLine - 1) * (gridCount - 1);
                Vector3 node = LatticeGrid.Node(axis, nodeIndex, i, j, gridCount, gridMin, gridMax);
                Vector3 warped = LatticeGrid.Warp(node, field, depthScale, maxDisplacement, zones);
                vertices[v] = warped + LatticeGrid.RingOffset(axis, side, sides) * radius;
            }

            mesh.SetVertices(vertices);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private void EnsureMesh()
        {
            if (mesh != null)
                return;

            mesh = meshFilter.mesh;
            vertices = new Vector3[mesh.vertexCount];
        }
    }
}
