using UnityEngine;
namespace DigitalLove.Game.Spaceships
{
    public class LineRendererEdges : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Transform origin;
        [SerializeField] private Transform destination;

        private void Update()
        {
            if (lineRenderer != null && origin != null && destination != null && lineRenderer.positionCount >= 2)
            {
                origin.transform.position = lineRenderer.GetPosition(0);
                origin.GetComponentInChildren<Renderer>().material.color = lineRenderer.material.color;
                destination.transform.position = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
                destination.GetComponentInChildren<Renderer>().material.color = lineRenderer.material.color;
            }
        }
    }
}