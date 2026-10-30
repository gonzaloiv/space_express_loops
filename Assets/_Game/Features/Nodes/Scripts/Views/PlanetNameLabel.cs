using TMPro;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class PlanetNameLabel : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Renderer planet;
        [Tooltip("Height of the label in metres, independent of the planet's size.")]
        [SerializeField] private float worldHeight = 0.008f;
        [Tooltip("Gap above the planet surface, as a fraction of the planet radius.")]
        [SerializeField] private float surfaceGap = 0.8f;
        [Tooltip("Distance in metres where the label is full size and opaque.")]
        [SerializeField] private float fadeNear = 0.4f;
        [Tooltip("Distance in metres where the label is smallest and transparent.")]
        [SerializeField] private float fadeFar = 1.6f;
        [SerializeField] private float farScale = 0.6f;

        private Camera view;

        private Camera View => view != null ? view : view = Camera.main;

        public void Place(string text)
        {
            label.text = text;
            label.enabled = !string.IsNullOrEmpty(text);
            transform.localPosition = new Vector3(0f, LocalHeight(), 0f);
            Apply();
        }

        public void Hide() => label.enabled = false;

        private float LocalHeight()
        {
            Bounds bounds = planet.localBounds;
            float radius = bounds.extents.y * planet.transform.localScale.y;
            return bounds.center.y * planet.transform.localScale.y + radius * (1f + surfaceGap);
        }

        private void LateUpdate() => Apply();

        private void Apply()
        {
            Camera camera = View;
            if (camera == null || !label.enabled)
                return;

            Vector3 centre = transform.position;
            transform.rotation = Quaternion.LookRotation(centre - camera.transform.position, Vector3.up);

            float distance = Vector3.Distance(camera.transform.position, centre);
            float fade = Mathf.InverseLerp(fadeNear, fadeFar, distance);
            float parentScale = transform.parent != null ? transform.parent.lossyScale.y : 1f;
            float scale = worldHeight * Mathf.Lerp(1f, farScale, fade) / Mathf.Max(parentScale, 0.00001f);
            transform.localScale = new Vector3(scale, scale, scale);
            label.alpha = 1f - fade;
        }
    }
}
