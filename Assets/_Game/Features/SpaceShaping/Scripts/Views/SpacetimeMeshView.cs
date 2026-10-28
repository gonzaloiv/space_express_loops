using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpacetimeMeshView : MonoBehaviour
    {
        [SerializeField] private float fieldMargin = 0.05f;

        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Mesh sourceMesh;
        [SerializeField] private int gridCount = 4;
        [SerializeField] private int sides = 6;
        [SerializeField] private float radius = 0.005f;
        [SerializeField] private float depthScale = 0.15f;
        [SerializeField] private float maxDisplacement = 0.08f;

        private Mesh mesh;
        private Vector3[] vertices;
        private int samplesPerLine;
        private SpacetimeField lastField;
        private IReadOnlyList<SpaceZone> lastZones;
        private float lastExtent;

        public int GridCount => gridCount;

        public int Sides => sides;

        public float Radius => radius;

        public void Render(SpacetimeField field, IReadOnlyList<SpaceZone> zones, float halfExtent)
        {
            lastField = field;
            lastZones = zones;
            lastExtent = halfExtent;
            EnsureMesh();

            float half = halfExtent / transform.lossyScale.x * (1f + fieldMargin);
            Vector3 gridMin = new(-half, -half, -half);
            Vector3 gridMax = new(half, half, half);

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
            meshRenderer.enabled = Application.isPlaying;
        }

        private void EnsureMesh()
        {
            if (mesh != null && mesh.vertexCount == sourceMesh.vertexCount)
                return;

            if (mesh != null)
            {
                if (Application.isPlaying)
                    Destroy(mesh);
                else
                    DestroyImmediate(mesh);
            }

            mesh = Instantiate(sourceMesh);
            mesh.MarkDynamic();
            if (!Application.isPlaying)
                mesh.hideFlags = HideFlags.HideAndDontSave;

            meshFilter.sharedMesh = mesh;
            vertices = new Vector3[mesh.vertexCount];
            samplesPerLine = mesh.vertexCount / (3 * gridCount * gridCount * sides);
        }

        private void OnValidate()
        {
            if (mesh != null && lastExtent > 0f)
                Render(lastField, lastZones, lastExtent);
        }

        private void OnDestroy()
        {
            if (mesh == null)
                return;

            if (Application.isPlaying)
                Destroy(mesh);
            else
                DestroyImmediate(mesh);
        }
    }
}
