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
        [SerializeField] private ColorValue neutral;
        [SerializeField] private ColorValue accent;
        [Tooltip("Opacity applied to the lattice colour (0 = invisible, 1 = the colour's own alpha).")]
        [Range(0f, 1f)]
        [SerializeField] private float gridOpacity = 0.35f;
        [SerializeField] private float tintStrength = 2.5f;

        private Mesh mesh;
        private Vector3[] vertices;
        private Color[] colors;
        private Vector3[] nodeFractions;
        private Vector3[] ringOffsets;
        private Bounds bounds;
        private int samplesPerLine;
        private RendererTint tint;
        private Func<Vector3, Vector3> lastWarp;
        private Func<Vector3, float> lastFade;
        private float lastStrength;
        private Vector3 lastCentre;
        private float lastHalf;

        public int GridCount => gridCount;

        public int Sides => sides;

        public float Radius => radius;

        public void Render(
            Func<Vector3, Vector3> warp, Func<Vector3, float> fade,
            float warpStrength, Vector3 centre, float half)
        {
            Remember(warp, fade, warpStrength, centre, half);
            EnsureMesh();
            transform.localPosition = centre;
            transform.localRotation = Quaternion.identity;
            WarpVertices(warp, fade, half);
            CommitMesh();
            ApplyTint(warpStrength);
        }

        private void ApplyTint(float strength)
        {
            float amount = Mathf.Clamp01(Mathf.Abs(strength) * tintStrength);
            Color color = Color.Lerp(neutral.value, accent.value, amount);
            color.a *= gridOpacity;
            tint.Set(meshRenderer, color);
        }

        private void Remember(
            Func<Vector3, Vector3> warp, Func<Vector3, float> fade,
            float warpStrength, Vector3 centre, float half)
        {
            lastWarp = warp;
            lastFade = fade;
            lastStrength = warpStrength;
            lastCentre = centre;
            lastHalf = half;
        }

        private void WarpVertices(Func<Vector3, Vector3> warp, Func<Vector3, float> fade, float half)
        {
            Matrix4x4 localToParent = transform.parent.worldToLocalMatrix * transform.localToWorldMatrix;
            Matrix4x4 parentToLocal = transform.worldToLocalMatrix * transform.parent.localToWorldMatrix;
            bounds = new Bounds(Vector3.zero, Vector3.zero);

            // A tube ring's `sides` vertices are consecutive and share one node: warp and fade it once.
            for (int v = 0; v < vertices.Length; v += sides)
            {
                Vector3 fraction = nodeFractions[v];
                Vector3 node = new(
                    (fraction.x * 2f - 1f) * half,
                    (fraction.y * 2f - 1f) * half,
                    (fraction.z * 2f - 1f) * half);
                Vector3 warped = warp(localToParent.MultiplyPoint3x4(node));
                WriteRing(v, parentToLocal.MultiplyPoint3x4(warped), new Color(1f, 1f, 1f, fade(warped)));
            }
        }

        private void WriteRing(int first, Vector3 centre, Color color)
        {
            for (int v = first; v < first + sides; v++)
            {
                Vector3 local = centre + ringOffsets[v] * radius;
                vertices[v] = local;
                colors[v] = color;
                bounds.Encapsulate(local);
            }
        }

        private void BuildStaticData()
        {
            int perLine = samplesPerLine * sides;
            int perAxisVertices = gridCount * gridCount * perLine;
            nodeFractions = new Vector3[vertices.Length];
            ringOffsets = new Vector3[vertices.Length];

            for (int v = 0; v < vertices.Length; v++)
            {
                LatticeSample sample = Decode(v, perLine, perAxisVertices);
                nodeFractions[v] = LatticeGrid.Node(
                    sample.Axis, sample.NodeIndex, sample.I, sample.J, gridCount,
                    Vector3.zero, Vector3.one);
                ringOffsets[v] = LatticeGrid.RingOffset(sample.Axis, sample.Side, sides);
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
            mesh.SetColors(colors);
            mesh.bounds = bounds;
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
            colors = new Color[mesh.vertexCount];
            samplesPerLine = mesh.vertexCount / (3 * gridCount * gridCount * sides);
            BuildStaticData();
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
            if (mesh != null && lastWarp != null && lastFade != null && lastHalf > 0f)
                Render(lastWarp, lastFade, lastStrength, lastCentre, lastHalf);
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
            if (mesh == null)
                return;

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
