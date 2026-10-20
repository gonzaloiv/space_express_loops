using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class LineRendererWithEdges : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Transform origin;
        [SerializeField] private Transform destination;
        [SerializeField] private Transform contentRoot;

        private bool visibleRequested;
        private Renderer originRenderer;
        private Renderer destinationRenderer;
        private float baseWidthMultiplier = 1f;

        private void Awake()
        {
            lineRenderer.useWorldSpace = false;
            baseWidthMultiplier = lineRenderer.widthMultiplier;
        }

        private void LateUpdate()
        {
            lineRenderer.widthMultiplier = baseWidthMultiplier * contentRoot.lossyScale.x;

            if (visibleRequested && lineRenderer.enabled)
                RefreshEdgeMarkers();
        }

        public void SetColor(Color color)
        {
            lineRenderer.material.color = color;
            RefreshEdgeMarkers();
        }

        public void SetVisible(bool visible)
        {
            visibleRequested = visible;
            lineRenderer.gameObject.SetActive(visible);

            if (!visible)
                Clear();
            else
                RefreshEdgeMarkers();
        }

        public void SetWorldTwoPositions(Vector3 start, Vector3 end) => ApplyWorldPositions(start, end);

        public void SetPositions(Vector3[] contentLocalPositions)
        {
            if (contentLocalPositions == null || contentLocalPositions.Length == 0)
            {
                Clear();
                return;
            }

            if (contentLocalPositions.Length == 2)
            {
                ApplyTwoPointPositions(contentLocalPositions[0], contentLocalPositions[1]);
                return;
            }

            ApplyPolylinePositions(contentLocalPositions);
        }

        private void ApplyTwoPointPositions(Vector3 start, Vector3 end) =>
            ApplyWorldPositions(ToWorld(start), ToWorld(end));

        private void ApplyPolylinePositions(Vector3[] contentLocalPositions)
        {
            lineRenderer.positionCount = contentLocalPositions.Length;
            for (int i = 0; i < contentLocalPositions.Length; i++)
                lineRenderer.SetPosition(i, lineRenderer.transform.InverseTransformPoint(ToWorld(contentLocalPositions[i])));

            lineRenderer.enabled = true;
            RefreshEdgeMarkers();
        }

        private Vector3 ToWorld(Vector3 contentLocalPosition) =>
            contentRoot.TransformPoint(contentLocalPosition);

        public void Clear()
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
            RefreshEdgeMarkers();
        }

        private void ApplyWorldPositions(Vector3 start, Vector3 end)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, lineRenderer.transform.InverseTransformPoint(start));
            lineRenderer.SetPosition(1, lineRenderer.transform.InverseTransformPoint(end));
            lineRenderer.enabled = true;
            RefreshEdgeMarkers();
        }

        private void RefreshEdgeMarkers()
        {
            bool showMarkers = visibleRequested
                && lineRenderer.gameObject.activeInHierarchy
                && lineRenderer.enabled
                && lineRenderer.positionCount >= 2;

            origin.gameObject.SetActive(showMarkers);
            destination.gameObject.SetActive(showMarkers);

            if (!showMarkers)
                return;

            Color color = lineRenderer.material.color;
            Vector3 start = lineRenderer.transform.TransformPoint(lineRenderer.GetPosition(0));
            Vector3 end = lineRenderer.transform.TransformPoint(lineRenderer.GetPosition(lineRenderer.positionCount - 1));
            SyncMarker(origin, ref originRenderer, start, color);
            SyncMarker(destination, ref destinationRenderer, end, color);
        }

        private static void SyncMarker(Transform marker, ref Renderer cachedRenderer, Vector3 position, Color color)
        {
            marker.position = position;
            cachedRenderer ??= marker.GetComponentInChildren<Renderer>();
            if (cachedRenderer != null)
                cachedRenderer.material.color = color;
        }
    }
}
