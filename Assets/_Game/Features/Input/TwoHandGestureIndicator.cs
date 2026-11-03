using UnityEngine;

namespace DigitalLove.Input
{
    public class TwoHandGestureIndicator : MonoBehaviour
    {
        [SerializeField] private TwoHandGestureInput gesture;
        [SerializeField] private LineRenderer handLine;
        [SerializeField] private Transform leftPinchMarker;
        [SerializeField] private Transform rightPinchMarker;
        [SerializeField] private float lineMaxMultiplier = 2.5f;
        [SerializeField] private float lineTensionSpeed = 0.6f;
        [SerializeField] private bool logIndicator;

        private bool wasVisible;

        private void Update()
        {
            TwoHandGestureState state = gesture.State;
            bool left = state.IsLeftPinching || state.IsActive;
            bool right = state.IsRightPinching || state.IsActive;

            handLine.enabled = left && right;
            ShowMarker(leftPinchMarker, state.Left.Point, left);
            ShowMarker(rightPinchMarker, state.Right.Point, right);

            bool visible = left || right;
            if (logIndicator && visible != wasVisible)
            {
                Debug.Log(
                    $"[GestureIndicator] visible={visible} L={left} R={right} " +
                    $"left={state.Left.Point} right={state.Right.Point}",
                    this);
                wasVisible = visible;
            }

            if (!handLine.enabled)
                return;

            float tension = Mathf.Clamp01(
                state.DistanceSpeed / Mathf.Max(lineTensionSpeed, 0.01f));
            handLine.widthMultiplier = Mathf.Lerp(1f, lineMaxMultiplier, tension);
            handLine.useWorldSpace = true;
            handLine.positionCount = 2;
            handLine.SetPosition(0, state.Left.Point);
            handLine.SetPosition(1, state.Right.Point);
        }

        private static void ShowMarker(Transform marker, Vector3 point, bool visible)
        {
            marker.gameObject.SetActive(visible);
            if (!visible)
                return;

            marker.position = point;
            Camera camera = Camera.main;
            if (camera != null)
                marker.rotation = camera.transform.rotation;
        }
    }
}
