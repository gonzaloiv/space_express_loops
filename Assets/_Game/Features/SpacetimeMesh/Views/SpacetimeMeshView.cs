using DigitalLove.Global;
using Unity.Profiling;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    // Draws the lattice: the baked tube mesh, bent by the field on Burst jobs every render.
    public class SpacetimeMeshView : MonoBehaviour, ISerializationCallbackReceiver
    {
        private static readonly ProfilerMarker WarpMarker = new("Shaping.Lattice.Warp");
        private static readonly ProfilerMarker UploadMarker = new("Shaping.Lattice.Upload");

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
        [Tooltip("Smoothing passes along each line on the warp: softens spikes without more points. 0 is off.")]
        [SerializeField, Range(0, 8)] private int smoothPasses = 2;
        [SerializeField] private LatticeStrainStyle strain = new();

        private Mesh mesh;
        private LatticeMeshBuffers buffers;
        private RendererTint tint;
        private ILatticeField lastField;
        private float lastStrength;
        private Vector3 lastCentre;
        private float lastHalf;

        public int GridCount => gridCount;

        public int Sides => sides;

        public float Radius => radius;

        public void Render(ILatticeField field, float warpStrength, Vector3 centre, float half)
        {
            Remember(field, warpStrength, centre, half);
            EnsureMesh();
            transform.localPosition = centre;
            transform.localRotation = Quaternion.identity;
            using (WarpMarker.Auto())
                buffers.Run(field, Pass(half));
            using (UploadMarker.Auto())
                CommitMesh();
            ApplyTint(warpStrength);
        }

        private LatticePass Pass(float half) =>
            new()
            {
                LocalToParent = transform.parent.worldToLocalMatrix * transform.localToWorldMatrix,
                ParentToLocal = transform.worldToLocalMatrix * transform.parent.localToWorldMatrix,
                Half = half,
                Radius = radius,
                SmoothPasses = smoothPasses,
                Look = strain.Look,
            };

        private void ApplyTint(float strength)
        {
            float amount = Mathf.Clamp01(Mathf.Abs(strength) * tintStrength);
            Color color = Color.Lerp(neutral.value, accent.value, amount);
            color.a *= gridOpacity;
            tint.Set(meshRenderer, color);
            strain.ApplyGlow(meshRenderer, accent.value);
        }

        private void Remember(ILatticeField field, float warpStrength, Vector3 centre, float half)
        {
            lastField = field;
            lastStrength = warpStrength;
            lastCentre = centre;
            lastHalf = half;
        }

        private void CommitMesh()
        {
            buffers.WriteTo(mesh);
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
            buffers = new LatticeMeshBuffers(mesh.vertexCount, gridCount, sides);
        }

        private bool IsMeshCurrent() =>
            mesh != null && buffers != null && mesh.vertexCount == sourceMesh.vertexCount;

        private void AttachMesh()
        {
            if (meshFilter.sharedMesh != mesh)
                meshFilter.sharedMesh = mesh;
        }

        private void ReleaseMesh()
        {
            ReleaseBuffers();
            if (mesh == null)
                return;

            if (meshFilter.sharedMesh == mesh)
                meshFilter.sharedMesh = sourceMesh;

            if (Application.isPlaying)
                Destroy(mesh);
            else
                DestroyImmediate(mesh);

            mesh = null;
        }

        private void ReleaseBuffers()
        {
            buffers?.Dispose();
            buffers = null;
        }

        private void OnValidate()
        {
            if (isActiveAndEnabled && mesh != null && lastField != null && lastField.IsReady && lastHalf > 0f)
                Render(lastField, lastStrength, lastCentre, lastHalf);
        }

        // The buffers are native: let them go with the component, and rebuild on the next render.
        private void OnDisable() => ReleaseBuffers();

        private void OnDestroy() => ReleaseMesh();

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
    }
}
