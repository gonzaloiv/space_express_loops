using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class PuzzleLabButton : MonoBehaviour
    {
        [SerializeField] private PuzzleLabPanel panel;
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private Renderer face;
        [SerializeField] private ColorValue neutral;
        [SerializeField] private ColorValue accent;
        [SerializeField] private float pressedSecs = 0.12f;

        private RendererTint tint;
        private Vector3 home;
        private bool hovered;
        private float pressedUntil;

        private void Awake() => home = transform.localPosition;

        private void OnEnable() => grabbable.WhenPointerEventRaised += OnPointerEvent;

        private void OnDisable() => grabbable.WhenPointerEventRaised -= OnPointerEvent;

        private void Update()
        {
            bool pressed = Time.unscaledTime < pressedUntil;
            tint.Set(face, pressed || hovered ? accent.value : neutral.value);
        }

        private void LateUpdate()
        {
            transform.localPosition = home;
            transform.localRotation = Quaternion.identity;
        }

        private void OnPointerEvent(PointerEvent evt)
        {
            switch (evt.Type)
            {
                case PointerEventType.Hover:
                    hovered = true;
                    break;
                case PointerEventType.Unhover:
                case PointerEventType.Cancel:
                    hovered = false;
                    break;
                case PointerEventType.Select:
                    pressedUntil = Time.unscaledTime + pressedSecs;
                    Activate();
                    break;
            }
        }

        private void Activate() => panel.ResetPuzzle();
    }
}
