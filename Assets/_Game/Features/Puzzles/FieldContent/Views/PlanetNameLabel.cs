using TMPro;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class PlanetNameLabel : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Renderer planet;
        [Tooltip("Height of the label in metres, independent of the planet's size.")]
        [SerializeField] private float worldHeight = 0.008f;
        [Tooltip("Gap in metres between the planet's surface and the label.")]
        [SerializeField] private float surfaceGap = 0.004f;
        [Tooltip("Distance in metres where the label is full size and opaque.")]
        [SerializeField] private float fadeNear = 0.4f;
        [Tooltip("Distance in metres where the label is smallest and transparent.")]
        [SerializeField] private float fadeFar = 1.6f;
        [SerializeField] private float farScale = 0.6f;

        private Camera view;
        private float fadeMultiplier = 1f;
        private Color tint;
        private Camera View => view != null ? view : view = Camera.main;

        private void Awake()
        {
            if (surfaceGap < 0f)
                surfaceGap = 0f;

            tint = label.color;
        }

        public void Place(string text)
        {
            label.text = text;
            label.enabled = !string.IsNullOrEmpty(text);
            Apply();
        }

        public void Hide() => label.enabled = false;

        public void SetFade(float amount) => fadeMultiplier = 1f - Mathf.Clamp01(amount);

        private void LateUpdate() => Apply();

        private void Apply()
        {
            transform.localPosition = OverPlanetOffset();

            Camera camera = View;
            if (camera == null || !label.enabled)
                return;

            FaceCamera(camera);
            ApplyScaleAndFade(camera);
        }

        private Vector3 OverPlanetOffset()
        {
            float parentScale = transform.parent != null ? transform.parent.lossyScale.y : 1f;
            float clearance = planet.bounds.extents.y + surfaceGap + worldHeight * 0.5f;
            return Vector3.up * (clearance / Mathf.Max(parentScale, 0.00001f));
        }

        private void FaceCamera(Camera camera)
        {
            Vector3 forward = transform.position - camera.transform.position;
            if (forward.sqrMagnitude < 0.000001f)
                return;

            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(forward.normalized, up)) > 0.999f)
                up = camera.transform.right;
            transform.rotation = Quaternion.LookRotation(forward, up);
        }

        private void ApplyScaleAndFade(Camera camera)
        {
            float distance = Vector3.Distance(camera.transform.position, transform.position);
            float fade = Mathf.InverseLerp(fadeNear, fadeFar, distance);
            float parentScale = transform.parent != null ? transform.parent.lossyScale.y : 1f;
            float scale = worldHeight * Mathf.Lerp(1f, farScale, fade) / Mathf.Max(parentScale, 0.00001f);
            transform.localScale = new Vector3(scale, scale, scale);
            label.color = tint;
            label.alpha = (1f - fade) * fadeMultiplier;
        }
    }
}
