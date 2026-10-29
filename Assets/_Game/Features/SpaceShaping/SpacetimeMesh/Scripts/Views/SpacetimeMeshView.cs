using System;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    public class SpacetimeMeshView : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Mesh sourceMesh;
        [SerializeField] private int gridCount = 4;
        [SerializeField] private int sides = 6;
        [SerializeField] private float radius = 0.005f;
        [SerializeField] private Color slowColor = new(0.45f, 0.7f, 1f, 1f);
        [SerializeField] private Color fastColor = new(1f, 0.65f, 0.35f, 1f);
        [SerializeField] private float tintStrength = 0.35f;

        private Mesh mesh;
        private Vector3[] vertices;
        private int samplesPerLine;
        private RendererTint tint;
        private Func<Vector3, Vector3> lastWarp;
        private float lastStrength;
        private Vector3 lastCentre;
        private float lastHalf;

        public int GridCount => gridCount;

        public int Sides => sides;

        public float Radius => radius;

        public void Render(
            Func<Vector3, Vector3> warp, float warpStrength, Vector3 centre, float half)
        {
            Remember(warp, warpStrength, centre, half);
            EnsureMesh();
            transform.localPosition = centre;
            transform.localRotation = Quaternion.identity;
            WarpVertices(warp, half);
            CommitMesh();
            ApplyTint(warpStrength);
        }

        private void ApplyTint(float strength)
        {
            Color target = strength >= 0f ? slowColor : fastColor;
            float amount = Mathf.Clamp01(Mathf.Abs(strength) * tintStrength);
            tint.Set(meshRenderer, Color.Lerp(Color.white, target, amount));
        }

        private void Remember(
            Func<Vector3, Vector3> warp, float warpStrength, Vector3 centre, float half)
        {
            lastWarp = warp;
            lastStrength = warpStrength;
            lastCentre = centre;
            lastHalf = half;
        }

        private void WarpVertices(Func<Vector3, Vector3> warp, float half)
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
                Vector3 warpNode = transform.parent.InverseTransformPoint(transform.TransformPoint(node));
                Vector3 warped = warp(warpNode);
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
            if (mesh != null && lastWarp != null && lastHalf > 0f)
                Render(lastWarp, lastStrength, lastCentre, lastHalf);
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
