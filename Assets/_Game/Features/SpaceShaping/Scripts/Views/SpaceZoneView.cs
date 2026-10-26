using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceZoneView : MonoBehaviour
    {
        [SerializeField] private float falloff = 0.5f;
        [SerializeField] private float contentInset = 0.8f;
        [SerializeField] private Transform contentVisual;

        private float scale = 1f;
        private bool scaling;

        public SpaceZone Zone => new(Centre, Size * 0.5f, scale, falloff);

        public Vector3 Centre => transform.localPosition;

        public Vector3 Size => transform.localScale;

        public float Scale => scale;

        public bool IsScaling => scaling;

        public Vector3 ContentSize
        {
            get
            {
                Vector3 content = Size - Vector3.one * contentInset;
                return new Vector3(
                    Mathf.Max(content.x, 0.1f),
                    Mathf.Max(content.y, 0.1f),
                    Mathf.Max(content.z, 0.1f));
            }
        }

        public Vector3 ContentHalfExtents => ContentSize * 0.5f;

        private void Awake() => ApplyContentVisual();

        private void OnValidate() => ApplyContentVisual();

        public void SetFrame(Vector3 centre, Vector3 size)
        {
            transform.localPosition = centre;
            transform.localScale = size;
            ApplyContentVisual();
        }

        public void SetScale(float value) => scale = Mathf.Max(value, 0.01f);

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
