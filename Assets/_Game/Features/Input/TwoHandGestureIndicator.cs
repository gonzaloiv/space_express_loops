using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Input
{
    public class TwoHandGestureIndicator : MonoBehaviour
    {
        [SerializeField] private TwoHandGestureInput gesture;
        [SerializeField] private LineRenderer handLine;
        [SerializeField] private LineRenderer leftPinchMarker;
        [SerializeField] private LineRenderer rightPinchMarker;
        [SerializeField] private LineRenderer startMarker;
        [SerializeField] private ColorValue swipeColor;
        [SerializeField] private ColorValue twoHandColor;
        [SerializeField] private ColorValue leftMarkerColor;
        [SerializeField] private ColorValue rightMarkerColor;
        [SerializeField] private float lineMaxMultiplier = 2.5f;
        [SerializeField] private float lineTensionSpeed = 0.6f;
        [SerializeField] private bool logIndicator;

        private Camera viewCamera;
        private bool wasVisible;

        // Set by the owner each frame: the markers draw only while a gesture that does something is
        // in progress, never for a stray pinch.
        public bool Shown { get; set; }
        private bool leftMarkerVisible;
        private bool rightMarkerVisible;
        private bool startMarkerVisible;

        private void Awake()
        {
            viewCamera = Camera.main;
        }

        private void OnDisable()
        {
            handLine.enabled = false;
            SetMarkerVisible(leftPinchMarker, false, ref leftMarkerVisible);
            SetMarkerVisible(rightPinchMarker, false, ref rightMarkerVisible);
            SetMarkerVisible(startMarker, false, ref startMarkerVisible);
            wasVisible = false;
        }

        private void LateUpdate()
        {
            if (viewCamera == null)
                viewCamera = Camera.main;

            TwoHandGestureState state = gesture.State;
            bool both = Shown && state.IsActive;
            bool single = Shown && state.IsSingleHandPinching && !both;
            bool left = Shown && (state.IsLeftPinching || both);
            bool right = Shown && (state.IsRightPinching || both);

            handLine.enabled = single || both;
            ShowMarkers(state, single, left, right);
            LogVisibility(state, single, both, left, right);

            if (handLine.enabled)
                DrawLine(state, single);
        }

        private void ShowMarkers(
            TwoHandGestureState state, bool single, bool left, bool right)
        {
            ShowMarker(leftPinchMarker, state.Left.Point, left, leftMarkerColor.value, ref leftMarkerVisible);
            ShowMarker(rightPinchMarker, state.Right.Point, right, rightMarkerColor.value, ref rightMarkerVisible);

            Color startColor = state.IsLeftPinching
                ? leftMarkerColor.value
                : rightMarkerColor.value;
            ShowMarker(
                startMarker, state.DragStartPoint, single,
                startColor, ref startMarkerVisible);
        }

        private void LogVisibility(
            TwoHandGestureState state, bool single, bool both, bool left, bool right)
        {
            bool visible = left || right;
            if (!logIndicator || visible == wasVisible)
                return;

            Debug.Log(
                $"[GestureIndicator] visible={visible} single={single} both={both} " +
                $"L={left} R={right} left={state.Left.Point} right={state.Right.Point}",
                this);
            wasVisible = visible;
        }

        private void DrawLine(TwoHandGestureState state, bool single)
        {
            handLine.useWorldSpace = true;
            handLine.positionCount = 2;
            if (single)
            {
                handLine.startColor = swipeColor.value;
                handLine.endColor = swipeColor.value;
                handLine.widthMultiplier = 1f;
                handLine.SetPosition(0, state.DragStartPoint);
                handLine.SetPosition(1, state.DragPoint);
                return;
            }

            float tension = Mathf.Clamp01(
                state.DistanceSpeed / Mathf.Max(lineTensionSpeed, 0.01f));
            handLine.startColor = twoHandColor.value;
            handLine.endColor = twoHandColor.value;
            handLine.widthMultiplier = Mathf.Lerp(1f, lineMaxMultiplier, tension);
            handLine.SetPosition(0, state.Left.Point);
            handLine.SetPosition(1, state.Right.Point);
        }

        private void ShowMarker(
            LineRenderer marker, Vector3 point, bool visible, Color color, ref bool markerVisible)
        {
            SetMarkerVisible(marker, visible, ref markerVisible);
            if (!visible)
                return;

            marker.transform.position = point;
            if (viewCamera != null)
                marker.transform.rotation = viewCamera.transform.rotation;

            marker.startColor = color;
            marker.endColor = color;
        }

        private static void SetMarkerVisible(
            LineRenderer marker, bool visible, ref bool markerVisible)
        {
            if (markerVisible == visible)
                return;

            marker.gameObject.SetActive(visible);
            markerVisible = visible;
        }
    }
}
