using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace DigitalLove.Game.UI.Console
{
    // Lays the console's windows out like one desktop around the seated player: every window on the
    // same arc around the head, facing it, top edges aligned, tiled outward from the field with the
    // same gap. The field owns the centre; the windows take the sides. Lives on the origin, so the
    // whole desk moves with it; the seat placement re-arranges it around the real head, so the windows
    // stay at eye level when the field is set lower.
    public class ConsoleLayout : MonoBehaviour
    {
        [Tooltip("The player's head in this transform's space until the seat is placed (and in the editor).")]
        [FormerlySerializedAs("head")]
        [SerializeField] private Vector3 defaultHead = new(0f, 0.15f, -0.45f);
        [Tooltip("Distance from the head to every window, in metres.")]
        [SerializeField, Min(0.1f)] private float radius = 0.5f;
        [Tooltip("Where the first window on each side starts, in degrees from straight ahead — clear of the field.")]
        [SerializeField, Range(0f, 80f)] private float innerAngle = 28f;
        [Tooltip("Gap between neighbouring windows, in degrees.")]
        [SerializeField, Range(0f, 10f)] private float gapAngle = 2f;
        [Tooltip("Height of every window's top edge above the eyes, in metres.")]
        [SerializeField] private float topAboveEyes = 0.05f;
        [Tooltip("Slots from the field outward, on the player's left.")]
        [SerializeField] private ConsoleSlot[] left;
        [Tooltip("Slots from the field outward, on the player's right.")]
        [SerializeField] private ConsoleSlot[] right;

        private Vector3 head;

        private void Awake() => ArrangeAroundDefault();

        // Lays the windows out around a head given in this transform's space.
        public void ArrangeAround(Vector3 localHead)
        {
            head = localHead;
            Tile(left, -1f);
            Tile(right, 1f);
        }

        [ContextMenu("Arrange")]
        private void ArrangeAroundDefault() => ArrangeAround(defaultHead);

        private void Tile(ConsoleSlot[] slots, float side)
        {
            float angle = innerAngle;
            foreach (ConsoleSlot slot in slots)
            {
                float span = Span(slot.Width);
                foreach (RectTransform window in slot.Windows)
                    Place(window, side * (angle + span * 0.5f));
                angle += span + gapAngle;
            }
        }

        private float Span(float width) =>
            2f * Mathf.Rad2Deg * Mathf.Asin(Mathf.Clamp(width / (2f * radius), 0f, 1f));

        private void Place(RectTransform window, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            Vector3 flat = new(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
            Vector3 centre = head + flat * radius;
            centre.y = head.y + topAboveEyes - ConsoleSlot.SizeOf(window).y * 0.5f;

            window.rotation = transform.rotation * Quaternion.LookRotation(centre - head);
            Vector3 target = transform.TransformPoint(centre);
            window.position += target - window.TransformPoint(window.rect.center);
        }
    }

    // One place on the desk. Windows that never show together (the map's and the job's contract)
    // share a slot, so the contract always appears in the same spot.
    [Serializable]
    public struct ConsoleSlot
    {
        [SerializeField] private RectTransform[] windows;

        public RectTransform[] Windows => windows;

        public float Width
        {
            get
            {
                float width = 0f;
                foreach (RectTransform window in windows)
                    width = Mathf.Max(width, SizeOf(window).x);
                return width;
            }
        }

        public static Vector2 SizeOf(RectTransform window) =>
            Vector2.Scale(window.rect.size, window.lossyScale);
    }
}
