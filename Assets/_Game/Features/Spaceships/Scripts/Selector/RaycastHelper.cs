using UnityEngine;
using DigitalLove.Game.Planets;
using System.Linq;

namespace DigitalLove.Game.Spaceships
{
    public class RaycastHelper : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float rayDistance = 2.5f;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float rayForwardOffset = 0.1f;

        private bool isActive = false;
        private PlanetBaseBehaviour basePlanet;

        [Header("DEBUG")]
        [SerializeField] private PlanetBehaviour candidatePlanet = null;

        public PlanetBehaviour CandidatePlanet => candidatePlanet;

        public void SetActive(bool isActive)
        {
            this.isActive = isActive;
            if (!isActive)
                candidatePlanet = null;
            SetVisible(isActive);
        }

        public void SetBase(PlanetBaseBehaviour basePlanet)
        {
            this.basePlanet = basePlanet;
        }

        public void SetVisible(bool isVisible)
        {
            lineRenderer.enabled = isVisible;
        }

        private void Update()
        {
            CheckHits();
            ShowLineRenderer();
        }

        private void CheckHits()
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, rayDistance, layerMask);
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
            }
            else
            {
                lineRenderer.positionCount = 2;
                Vector3 startPosition = transform.parent.parent.position + transform.forward * rayForwardOffset;
                Vector3 endPosition = startPosition + transform.forward * rayDistance;
                if (candidatePlanet != null)
                {

                    float distance = Vector3.Distance(candidatePlanet.transform.position, startPosition);
                    Vector3 offset = transform.forward * candidatePlanet.PlanetBody.RadiusOffset - transform.forward * rayForwardOffset;
                    endPosition = startPosition + transform.forward * distance - offset;
                }
                lineRenderer.SetPositions(new Vector3[] { startPosition, endPosition });
                lineRenderer.enabled = true;
            }
        }
    }

}