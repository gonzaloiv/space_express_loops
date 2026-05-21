using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class LineRendererEdges : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Transform origin;
        [SerializeField] private Transform destination;

        private bool visibleRequested;

        private void Start() => RefreshEdgeMarkers();

        public void SetVisible(bool isVisible)
        {
            visibleRequested = isVisible;
            RefreshEdgeMarkers();
        }

        private void Update()
        {
            RefreshEdgeMarkers();
        }

        private void RefreshEdgeMarkers()
        {
            if (origin == null || destination == null)
                return;

            bool showMarkers = visibleRequested
                && lineRenderer != null
                && lineRenderer.gameObject.activeInHierarchy
                && lineRenderer.enabled
                && lineRenderer.positionCount >= 2;

            origin.gameObject.SetActive(showMarkers);
            destination.gameObject.SetActive(showMarkers);

            if (!showMarkers)
                return;

            origin.position = lineRenderer.GetPosition(0);
            origin.GetComponentInChildren<Renderer>().material.color = lineRenderer.material.color;
            destination.position = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
            destination.GetComponentInChildren<Renderer>().material.color = lineRenderer.material.color;
        }
    }
}