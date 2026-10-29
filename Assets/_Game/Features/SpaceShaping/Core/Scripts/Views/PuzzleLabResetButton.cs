using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class PuzzleLabResetButton : MonoBehaviour
    {
        [SerializeField] protected PuzzleLabPanel panel;
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private Renderer face;
        [SerializeField] private Color idleColor = new(0.14117648f, 0.14117648f, 0.14117648f, 1f);
        [SerializeField] private Color hoverColor = new(0.1882353f, 0.1882353f, 0.1882353f, 1f);
        [SerializeField] private Color pressedColor = new(0.23137255f, 0.36078432f, 0.9647059f, 1f);
        [SerializeField] private float pressedSecs = 0.12f;

        private RendererTint tint;
        private Vector3 home;
        private bool hovered;
        private float pressedUntil;

        protected virtual void Awake() => home = transform.localPosition;

        protected virtual void OnEnable() => grabbable.WhenPointerEventRaised += OnPointerEvent;

        protected virtual void OnDisable() => grabbable.WhenPointerEventRaised -= OnPointerEvent;

        protected virtual void Update()
        {
            bool pressed = Time.unscaledTime < pressedUntil;
            tint.Set(face, pressed ? pressedColor : hovered ? hoverColor : idleColor);
        }

        protected virtual void LateUpdate()
        {
            transform.localPosition = home;
            transform.localRotation = Quaternion.identity;
        }

        protected virtual void OnPointerEvent(PointerEvent evt)
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

        protected virtual void Activate() => panel.ResetPuzzle();
    }
}
