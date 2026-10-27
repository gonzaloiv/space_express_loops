using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpacetimeMeshView : MonoBehaviour
    {
        [SerializeField] private float fieldMargin = 0.05f;
        [SerializeField, Tooltip("Depth (Y) as a fraction of the run half-length. Set to 1 (with Width 1) for a cube.")]
        private float depthRatio = 0.6f;
        [SerializeField, Tooltip("Width (Z) as a fraction of the run half-length. Set to 1 (with Depth 1) for a cube.")]
        private float widthRatio = 0.5f;

        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private int gridCount = 4;
        [SerializeField] private int sides = 6;
        [SerializeField] private float radius = 0.005f;
        [SerializeField] private float depthScale = 0.15f;
        [SerializeField] private float maxDisplacement = 0.08f;

        private Mesh mesh;
        private Vector3[] vertices;
        private int samplesPerLine;

        public void Render(SpacetimeField field, IReadOnlyList<SpaceZone> zones, float halfExtent)
        {
            EnsureMesh();

            float half = halfExtent / transform.lossyScale.x * (1f + fieldMargin);
            Vector3 gridMin = new(-half, -half * depthRatio, -half * widthRatio);
            Vector3 gridMax = new(half, half * depthRatio, half * widthRatio);

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

            mesh = Instantiate(meshFilter.sharedMesh);
            mesh.MarkDynamic();
            meshFilter.sharedMesh = mesh;
            vertices = new Vector3[mesh.vertexCount];
            samplesPerLine = mesh.vertexCount / (3 * gridCount * gridCount * sides);
        }

        private void OnDestroy()
        {
            if (mesh == null || !Application.isPlaying)
                return;

            Destroy(mesh);
        }
    }
}
