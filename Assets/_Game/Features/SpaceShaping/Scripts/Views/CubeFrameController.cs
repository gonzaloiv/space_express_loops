using DigitalLove.Input;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class CubeFrameController : MonoBehaviour
    {
        [SerializeField] private TwoHandGestureInput gestureInput;
        [SerializeField] private SpaceZoneView zone;
        [SerializeField] private Transform container;
        [SerializeField] private float minSize = 0.5f;
        [SerializeField] private float maxSize = 8f;

        private bool isFraming;
        private Vector3 fixedCorner;

        private void Update()
        {
            if (gestureInput.State.IsSingleHandPinching)
            {
                if (!isFraming)
                    Begin(gestureInput.State.DragPoint);
                Apply(gestureInput.State.DragPoint);
                return;
            }

            isFraming = false;
        }

        private void Begin(Vector3 worldPoint)
        {
            isFraming = true;
            Vector3 offset = ToLocal(worldPoint) - zone.Centre;
            Vector3 sign = new(Sign(offset.x), Sign(offset.y), Sign(offset.z));
            fixedCorner = zone.Centre - Vector3.Scale(sign, zone.Size * 0.5f);
        }

        private void Apply(Vector3 worldPoint)
        {
            Vector3 grabbed = ToLocal(worldPoint);
            Vector3 size = Clamp(Abs(grabbed - fixedCorner));
            zone.SetFrame((grabbed + fixedCorner) * 0.5f, size);
        }

        private Vector3 ToLocal(Vector3 worldPoint) => container.InverseTransformPoint(worldPoint);

        private Vector3 Clamp(Vector3 size) => new(
            Mathf.Clamp(size.x, minSize, maxSize),
            Mathf.Clamp(size.y, minSize, maxSize),
            Mathf.Clamp(size.z, minSize, maxSize));

        private static float Sign(float value) => value >= 0f ? 1f : -1f;

        private static Vector3 Abs(Vector3 value) =>
            new(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }
}
