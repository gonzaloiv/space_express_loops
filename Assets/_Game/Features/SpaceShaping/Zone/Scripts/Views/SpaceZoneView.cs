using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    public class SpaceZoneView : MonoBehaviour
    {
        [SerializeField] private float falloff = 0.5f;
        [SerializeField] private float contentInsetFraction = 0.05f;
        [SerializeField] private Transform contentVisual;
        [SerializeField] private ZoneGrabListener listener;

        private float scale = 1f;
        private bool scaling;
        private ZoneConditions conditions;

        public SpaceZone Zone => new(Centre, Size * 0.5f, scale, falloff);

        public Vector3 Centre => transform.localPosition;

        public Vector3 Size => transform.localScale;

        public float Scale => scale;

        public bool IsScaling => scaling;

        public bool CanMove => conditions.Translatable;

        public bool CanResize => conditions.Resizable;

        public bool CanPlace(Vector3 centre, Vector3 size)
        {
            SpaceZone candidate = new(centre, size * 0.5f, scale, falloff);
            return !candidate.Influences(conditions.Origin)
                && !candidate.Influences(conditions.Destination);
        }

        private Vector3 ContentSize
        {
            get
            {
                Vector3 content = Size * (1f - contentInsetFraction);
                return new Vector3(
                    Mathf.Max(content.x, 0.1f),
                    Mathf.Max(content.y, 0.1f),
                    Mathf.Max(content.z, 0.1f));
            }
        }

        public bool ContentContains(Vector3 world)
        {
            Transform parent = transform.parent;
            Vector3 local = parent != null ? parent.InverseTransformPoint(world) : world;
            Vector3 offset = local - Centre;
            Vector3 half = ContentSize * 0.5f;
            return Mathf.Abs(offset.x) <= half.x
                && Mathf.Abs(offset.y) <= half.y
                && Mathf.Abs(offset.z) <= half.z;
        }

        private void Awake() => ApplyContentVisual();

        private void OnValidate() => ApplyContentVisual();

        public void SetFrame(Vector3 centre, Vector3 size)
        {
            transform.localPosition = centre;
            transform.localScale = size;
            ApplyContentVisual();
        }

        public void SetConditions(ZoneConditions value) => conditions = value;

        public void SetFalloff(float value) => falloff = Mathf.Max(value, 0f);

        public void SetScale(float value) => scale = Mathf.Max(value, 0.01f);

        public void SetScaleEnvelope(float min, float max) => listener.SetScaleEnvelope(min, max);

        public void SetPrediction(ZoneVerdict verdict) => listener.SetPrediction(verdict);

        public void Refuse() => listener.Refuse();

        public void SetScaling(bool value) => scaling = value;

        public void ResetScale()
        {
            scale = 1f;
            scaling = false;
        }

        private void ApplyContentVisual()
        {
            if (contentVisual == null)
                return;

            Vector3 size = Size;
            Vector3 content = ContentSize;
            contentVisual.localScale = new Vector3(
                SafeRatio(content.x, size.x),
                SafeRatio(content.y, size.y),
                SafeRatio(content.z, size.z));
        }

        private static float SafeRatio(float content, float size) =>
            size > 0.0001f ? content / size : 1f;
    }
}
