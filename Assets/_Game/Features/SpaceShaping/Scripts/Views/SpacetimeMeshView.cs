using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping.Cube;
using DigitalLove.Global;
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
        [SerializeField] private Color slowColor = new(0.45f, 0.7f, 1f, 1f);
        [SerializeField] private Color fastColor = new(1f, 0.65f, 0.35f, 1f);
        [SerializeField] private float clockTintStrength = 0.35f;

        private Mesh mesh;
        private Vector3[] vertices;
        private int samplesPerLine;
        private RendererTint tint;
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
            Remember(field, zones, origin, destination, crossHalf);
            EnsureMesh();
            Place(origin, destination);
            WarpVertices(field, zones, HalfExtent(origin, destination, crossHalf));
            CommitMesh();
            ApplyClockTint(zones);
        }

        private void ApplyClockTint(IReadOnlyList<SpaceZone> zones)
        {
            float strongest = 1f;
            for (int i = 0; i < zones.Count; i++)
            {
                if (Mathf.Abs(zones[i].Scale - 1f) > Mathf.Abs(strongest - 1f))
                    strongest = zones[i].Scale;
            }

            Color target = strongest >= 1f ? slowColor : fastColor;
            float amount = Mathf.Clamp01(Mathf.Abs(strongest - 1f) * clockTintStrength);
            tint.Set(meshRenderer, Color.Lerp(Color.white, target, amount));
        }

        private void Remember(
            SpacetimeField field, IReadOnlyList<SpaceZone> zones, Vector3 origin, Vector3 destination,
            float crossHalf)
        {
            lastField = field;
            lastZones = zones;
            lastOrigin = origin;
            lastDestination = destination;
            lastCrossHalf = crossHalf;
        }

        private void Place(Vector3 origin, Vector3 destination)
        {
            transform.localPosition = (origin + destination) * 0.5f;
            transform.localRotation = Quaternion.identity;
        }

        private float HalfExtent(Vector3 origin, Vector3 destination, float crossHalf)
        {
            float length = (destination - origin).magnitude;
            return Mathf.Max(length * 0.5f + fieldMargin * length, crossHalf + latticePadding);
        }

        private void WarpVertices(SpacetimeField field, IReadOnlyList<SpaceZone> zones, float half)
        {
            Vector3 gridMin = new(-half, -half, -half);
            Vector3 gridMax = new(half, half, half);
            int perLine = samplesPerLine * sides;
            int perAxisVertices = gridCount * gridCount * perLine;

            for (int v = 0; v < vertices.Length; v++)
            {
                LatticeSample sample = Decode(v, perLine, perAxisVertices);
                Vector3 node = LatticeGrid.Node(
                    sample.Axis, sample.NodeIndex, sample.I, sample.J, gridCount, gridMin, gridMax);
                Vector3 puzzleNode = transform.parent.InverseTransformPoint(transform.TransformPoint(node));
                Vector3 warped = LatticeGrid.Warp(
                    puzzleNode, field, depthScale, maxDisplacement, zones, zoneShells);
                Vector3 local = transform.InverseTransformPoint(transform.parent.TransformPoint(warped));
                vertices[v] = local + LatticeGrid.RingOffset(sample.Axis, sample.Side, sides) * radius;
            }
        }

        private LatticeSample Decode(int vertex, int perLine, int perAxisVertices)
        {
            int axis = vertex / perAxisVertices;
            int rem = vertex % perAxisVertices;
            int line = rem / perLine;
            int rem2 = rem % perLine;
            float nodeIndex = (float)(rem2 / sides) / (samplesPerLine - 1) * (gridCount - 1);
            return new LatticeSample(
                axis, line / gridCount, line % gridCount, rem2 % sides, nodeIndex);
        }

        private void CommitMesh()
        {
            mesh.SetVertices(vertices);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshRenderer.enabled = Application.isPlaying;
        }

        private void EnsureMesh()
        {
            if (IsMeshCurrent())
            {
                AttachMesh();
                return;
            }

            ReleaseMesh();
            mesh = Instantiate(sourceMesh);
            mesh.MarkDynamic();
            if (!Application.isPlaying)
                mesh.hideFlags = HideFlags.HideAndDontSave;

            meshFilter.sharedMesh = mesh;
            vertices = new Vector3[mesh.vertexCount];
            samplesPerLine = mesh.vertexCount / (3 * gridCount * gridCount * sides);
        }

        private bool IsMeshCurrent() => mesh != null && mesh.vertexCount == sourceMesh.vertexCount;

        private void AttachMesh()
        {
            if (meshFilter.sharedMesh != mesh)
                meshFilter.sharedMesh = mesh;
        }

        private void ReleaseMesh()
        {
            if (mesh == null)
                return;

            if (Application.isPlaying)
                Destroy(mesh);
            else
                DestroyImmediate(mesh);
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

        private readonly struct LatticeSample
        {
            public readonly int Axis;
            public readonly int I;
            public readonly int J;
            public readonly int Side;
            public readonly float NodeIndex;

            public LatticeSample(int axis, int i, int j, int side, float nodeIndex)
            {
                Axis = axis;
                I = i;
                J = j;
                Side = side;
                NodeIndex = nodeIndex;
            }
        }
    }
}
