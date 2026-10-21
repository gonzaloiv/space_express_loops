using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class UniverseGhostView : MonoBehaviour
    {
        [SerializeField] private LineRenderer handLine;
        [SerializeField] private Transform leftPinchMarker;
        [SerializeField] private Transform rightPinchMarker;

        public void Show(
            Vector3 leftPoint,
            Vector3 rightPoint,
            bool leftPinching,
            bool rightPinching)
        {
            gameObject.SetActive(true);
            ShowMarker(leftPinchMarker, leftPoint, leftPinching);
            ShowMarker(rightPinchMarker, rightPoint, rightPinching);
            UpdateLine(leftPoint, rightPoint, leftPinching && rightPinching);
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

        private void UpdateLine(Vector3 first, Vector3 second, bool visible)
        {
            handLine.enabled = visible;
            if (!visible)
                return;

            handLine.useWorldSpace = true;
            handLine.positionCount = 2;
            handLine.SetPosition(0, first);
            handLine.SetPosition(1, second);
        }
    }
}
