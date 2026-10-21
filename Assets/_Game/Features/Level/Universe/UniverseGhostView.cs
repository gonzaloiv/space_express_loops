using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class UniverseGhostView : MonoBehaviour
    {
        [SerializeField] private LineRenderer handLine;
        [SerializeField] private Transform leftPinchMarker;
        [SerializeField] private Transform rightPinchMarker;
        [SerializeField] private float lineMaxMultiplier = 2.5f;
        [SerializeField] private float lineTensionSpeed = 0.6f;

        public void Show(
            Vector3 leftPoint,
            Vector3 rightPoint,
            bool leftPinching,
            bool rightPinching,
            float handSpeed)
        {
            gameObject.SetActive(true);
            ShowMarker(leftPinchMarker, leftPoint, leftPinching);
            ShowMarker(rightPinchMarker, rightPoint, rightPinching);
            UpdateLine(leftPoint, rightPoint, leftPinching && rightPinching, handSpeed);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
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

        private void UpdateLine(Vector3 first, Vector3 second, bool visible, float handSpeed)
        {
            handLine.enabled = visible;
            if (!visible)
                return;

            float tension = Mathf.Clamp01(handSpeed / Mathf.Max(lineTensionSpeed, 0.01f));
            handLine.widthMultiplier = Mathf.Lerp(1f, lineMaxMultiplier, tension);
            handLine.useWorldSpace = true;
            handLine.positionCount = 2;
            handLine.SetPosition(0, first);
            handLine.SetPosition(1, second);
        }
    }
}
