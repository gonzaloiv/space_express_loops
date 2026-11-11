using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // A faint authored box around one endpoint: the volume the cube may not cover. Flashes danger
    // when a move is refused on this endpoint.
    public class EndpointKeepOutView : MonoBehaviour
    {
        [SerializeField] private Renderer body;
        [SerializeField] private ColorValue idle;
        [SerializeField] private ColorValue danger;
        [SerializeField, Range(0f, 1f)] private float idleAlpha = 0.15f;
        [SerializeField] private float flashSecs = 0.18f;

        private RendererTint tint;
        private float flashUntil;
        private bool flashing;

        public void Show(Vector3 position, Vector3 size)
        {
            transform.localPosition = position;
            transform.localScale = size;
            if (gameObject.activeSelf)
                return;

            flashing = false;
            flashUntil = 0f;
            Apply(idle.value.WithAlpha(idleAlpha));
            gameObject.SetActive(true);
        }

        public void Flash()
        {
            flashing = true;
            flashUntil = Time.unscaledTime + flashSecs;
            Apply(danger.value);
        }

        public void Hide()
        {
            flashing = false;
            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (!flashing || Time.unscaledTime < flashUntil)
                return;

            flashing = false;
            Apply(idle.value.WithAlpha(idleAlpha));
        }

        private void Apply(Color color) => tint.Set(body, color);
    }
}
