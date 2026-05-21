using UnityEngine;
using DigitalLove.Game.Planets;
using System.Linq;
using DigitalLove.Global;

namespace DigitalLove.Game.Spaceships
{
    public class RaycastHelper : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private LineRendererEdges lineRendererEdges;
        [SerializeField] private FloatValue rayDistance;

        [SerializeField] private float rayForwardOffset = 0.1f;

        private bool isActive = false;
        private PlanetBehaviour candidatePlanet;

        public PlanetBehaviour CandidatePlanet => candidatePlanet;
        public float RayDistance => rayDistance.value * 1.1f;

        public void SetActive(bool isActive)
        {
            this.isActive = isActive;
            if (!isActive)
                candidatePlanet = null;
            SetVisible(isActive);
        }

        public void SetColor(Color color)
        {
            lineRenderer.material.color = color;
        }

        public void SetVisible(bool isVisible)
        {
            lineRenderer.gameObject.SetActive(isVisible);
            lineRendererEdges.SetVisible(isVisible);
        }

        private void Update()
        {
            if (!isActive)
                return;

            CheckHits();
            ShowLineRenderer();
        }

        private void CheckHits()
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, RayDistance, layerMask);
            RaycastHit[] planetHits = hits.Where(hit => hit.rigidbody != null && hit.rigidbody.GetComponent<PlanetBehaviour>() != null).ToArray();
            if (planetHits.Length > 0)
            {
                RaycastHit closestHit = planetHits.OrderBy(hit => Vector3.Distance(transform.position, hit.point)).FirstOrDefault();
                candidatePlanet = closestHit.rigidbody.GetComponent<PlanetBehaviour>();
            }
            else
            {
                candidatePlanet = null;
            }
        }

        private void ShowLineRenderer()
        {
            if (!isActive)
            {
                lineRenderer.positionCount = 0;
                lineRenderer.enabled = false;
                lineRendererEdges.SetVisible(false);
            }
            else
            {
                lineRenderer.positionCount = 2;
                Vector3 startPosition = transform.parent.parent.position + transform.forward * rayForwardOffset;
                Vector3 endPosition = startPosition + transform.forward * RayDistance;
                if (candidatePlanet != null)
                {

                    float distance = Vector3.Distance(candidatePlanet.transform.position, startPosition);
                    Vector3 offset = transform.forward * candidatePlanet.PlanetBody.Radius - transform.forward * rayForwardOffset;
                    endPosition = startPosition + transform.forward * distance - offset;
                }
                lineRenderer.SetPositions(new Vector3[] { startPosition, endPosition });
                lineRenderer.enabled = true;
                lineRendererEdges.SetVisible(true);
            }
        }
    }

}