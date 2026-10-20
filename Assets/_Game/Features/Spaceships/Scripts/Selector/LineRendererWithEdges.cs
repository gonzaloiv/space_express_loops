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
            if (contentRoot == null)
                contentRoot = transform.root;
            if (lineRenderer != null)
            {
                lineRenderer.useWorldSpace = false;
                baseWidthMultiplier = lineRenderer.widthMultiplier;
            }
        }

        private void LateUpdate()
        {
            if (lineRenderer == null)
                return;

            float scale = contentRoot != null ? contentRoot.lossyScale.x : 1f;
            lineRenderer.widthMultiplier = baseWidthMultiplier * scale;

            if (visibleRequested && lineRenderer.enabled)
                RefreshEdgeMarkers();
        }

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
                ApplyWorldPositions(
                    contentRoot != null ? contentRoot.TransformPoint(contentLocalPositions[0]) : contentLocalPositions[0],
                    contentRoot != null ? contentRoot.TransformPoint(contentLocalPositions[1]) : contentLocalPositions[1]);
                return;
            }

            if (lineRenderer == null)
                return;

            lineRenderer.positionCount = contentLocalPositions.Length;
            for (int i = 0; i < contentLocalPositions.Length; i++)
            {
                Vector3 world = contentRoot != null
                    ? contentRoot.TransformPoint(contentLocalPositions[i])
                    : contentLocalPositions[i];
                lineRenderer.SetPosition(i, lineRenderer.transform.InverseTransformPoint(world));
            }

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

        private void ApplyWorldPositions(Vector3 start, Vector3 end)
        {
            if (lineRenderer == null)
                return;

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, lineRenderer.transform.InverseTransformPoint(start));
            lineRenderer.SetPosition(1, lineRenderer.transform.InverseTransformPoint(end));
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
