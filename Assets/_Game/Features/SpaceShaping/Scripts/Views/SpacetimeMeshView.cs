using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpacetimeMeshView : MonoBehaviour, ISerializationCallbackReceiver
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
        [SerializeField] private float zoneShells = 3f;
        [SerializeField] private float latticePadding = 0.02f;

        private Mesh mesh;
        private Vector3[] vertices;
        private int samplesPerLine;
        private SpacetimeField lastField;
        private IReadOnlyList<SpaceZone> lastZones;
        private Vector3 lastOrigin;
        private Vector3 lastDestination;
        private float lastCrossHalf;

        public int GridCount => gridCount;

        public int Sides => sides;

        public float Radius => radius;

        public void Render(
            SpacetimeField field, IReadOnlyList<SpaceZone> zones, Vector3 origin, Vector3 destination,
            float crossHalf)
        {
            lastField = field;
            lastZones = zones;
            lastOrigin = origin;
            lastDestination = destination;
            lastCrossHalf = crossHalf;
            EnsureMesh();

            Vector3 run = destination - origin;
            float length = run.magnitude;
            transform.localPosition = (origin + destination) * 0.5f;
            transform.localRotation = Quaternion.identity;

            float half = Mathf.Max(length * 0.5f + fieldMargin * length, crossHalf + latticePadding);
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
                Vector3 puzzleNode = transform.parent.InverseTransformPoint(transform.TransformPoint(node));
                Vector3 warped = LatticeGrid.Warp(
                    puzzleNode, field, depthScale, maxDisplacement, zones, zoneShells);
                Vector3 localWarped = transform.InverseTransformPoint(transform.parent.TransformPoint(warped));
                vertices[v] = localWarped + LatticeGrid.RingOffset(axis, side, sides) * radius;
            }

            mesh.SetVertices(vertices);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshRenderer.enabled = Application.isPlaying;
        }

        private void EnsureMesh()
        {
            if (mesh != null && mesh.vertexCount == sourceMesh.vertexCount)
            {
                if (meshFilter.sharedMesh != mesh)
                    meshFilter.sharedMesh = mesh;

                return;
            }

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
            if (mesh != null && lastCrossHalf > 0f
                && (lastDestination - lastOrigin).sqrMagnitude > 0.000001f)
                Render(lastField, lastZones, lastOrigin, lastDestination, lastCrossHalf);
        }

        private void OnDestroy()
        {
            if (mesh == null)
                return;

            if (meshFilter.sharedMesh == mesh)
                meshFilter.sharedMesh = sourceMesh;

            if (Application.isPlaying)
                Destroy(mesh);
            else
                DestroyImmediate(mesh);
        }

        public void OnBeforeSerialize()
        {
            if (meshFilter.sharedMesh == mesh)
                meshFilter.sharedMesh = sourceMesh;
        }

        public void OnAfterDeserialize()
        {
        }
    }
}
