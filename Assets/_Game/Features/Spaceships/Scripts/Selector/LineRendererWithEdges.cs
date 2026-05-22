using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class LineRendererWithEdges : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Transform origin;
        [SerializeField] private Transform destination;

        private bool visibleRequested;
        private Renderer originRenderer;
        private Renderer destinationRenderer;

        public void SetColor(Color color)
        {
            if (lineRenderer == null)
                return;

            lineRenderer.material.color = color;
            RefreshEdgeMarkers();
        }

        public void SetVisible(bool visible)
        {
            visibleRequested = visible;
            if (lineRenderer != null)
                lineRenderer.gameObject.SetActive(visible);

            if (!visible)
                Clear();
            else
                RefreshEdgeMarkers();
        }

        public void SetTwoPositions(Vector3 start, Vector3 end) => ApplyPositions(start, end);

        public void SetPositions(Vector3[] positions)
        {
            if (positions == null || positions.Length == 0)
            {
                Clear();
                return;
            }

            if (positions.Length == 2)
            {
                ApplyPositions(positions[0], positions[1]);
                return;
            }

            if (lineRenderer == null)
                return;

            lineRenderer.positionCount = positions.Length;
            for (int i = 0; i < positions.Length; i++)
                lineRenderer.SetPosition(i, positions[i]);

            lineRenderer.enabled = true;
            RefreshEdgeMarkers();
        }

        public void Clear()
        {
            if (lineRenderer == null)
                return;

            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
            RefreshEdgeMarkers();
        }

        private void ApplyPositions(Vector3 start, Vector3 end)
        {
            if (lineRenderer == null)
                return;

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            lineRenderer.enabled = true;
            RefreshEdgeMarkers();
        }

        private void RefreshEdgeMarkers()
        {
            if (origin == null || destination == null || lineRenderer == null)
                return;

            bool showMarkers = visibleRequested
                && lineRenderer.gameObject.activeInHierarchy
                && lineRenderer.enabled
                && lineRenderer.positionCount >= 2;

            origin.gameObject.SetActive(showMarkers);
            destination.gameObject.SetActive(showMarkers);

            if (!showMarkers)
                return;

            Color color = lineRenderer.material.color;
            SyncMarker(origin, ref originRenderer, lineRenderer.GetPosition(0), color);
            SyncMarker(destination, ref destinationRenderer, lineRenderer.GetPosition(lineRenderer.positionCount - 1), color);
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
