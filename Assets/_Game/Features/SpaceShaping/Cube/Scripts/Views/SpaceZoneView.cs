using System;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    public class SpaceZoneView : MonoBehaviour
    {
        [SerializeField] private float falloff = 0.5f;
        [SerializeField] private float contentInsetFraction = 0.05f;
        [Tooltip("Extra volume around the content, as a fraction of its half-size, where a pinch still scales instead of grabbing the frame.")]
        [SerializeField] private float scaleMarginFraction = 0.25f;
        [SerializeField] private float throbAmount = 0.5f;
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

        // The content's share of the frame. Proportional, so the fill tracks any cube size; the old
        // absolute floor blew small cubes past the frame.
        private float ContentFraction => Mathf.Clamp(1f - contentInsetFraction, 0.01f, 1f);

        private Vector3 ContentSize => Size * ContentFraction;

        public Vector3 ToLocal(Vector3 world)
        {
            Transform parent = transform.parent;
            return parent != null ? parent.InverseTransformPoint(world) : world;
        }

        public bool ContentContains(Vector3 world) => Contains(world, ContentSize * 0.5f);

        public bool ScaleContains(Vector3 world) =>
            Contains(world, ContentSize * 0.5f * (1f + Mathf.Max(scaleMarginFraction, 0f)));

        private bool Contains(Vector3 world, Vector3 half)
        {
            Vector3 offset = ToLocal(world) - Centre;
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

        public void SetScale(float value)
        {
            scale = Mathf.Max(value, 0.01f);
            ApplyContentVisual();
        }

        public void SetScaleEnvelope(float min, float max) => listener.SetScaleEnvelope(min, max);

        public void SetPrediction(ZoneVerdict verdict) => listener.SetPrediction(verdict);

        public void ClearPrediction() => listener.ClearPrediction();

        public event Action<ZoneRefusal> Refused
        {
            add => listener.Refused += value;
            remove => listener.Refused -= value;
        }

        public void Refuse(ZoneRefusal reason) => listener.Refuse(reason);

        public void SetScaling(bool value) => scaling = value;

        public void ResetScale()
        {
            scale = 1f;
            scaling = false;
            ApplyContentVisual();
        }

        private void ApplyContentVisual()
        {
            if (contentVisual == null)
                return;

            float throb = 1f + Mathf.Abs(scale - 1f) * throbAmount;
            contentVisual.localScale = Vector3.one * Mathf.Min(ContentFraction * throb, 1f);
        }
    }
}
