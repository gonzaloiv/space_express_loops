using System;
using DigitalLove.Game.Planets;
using DigitalLove.Global;
using UnityEngine;
using UnityEngine.Splines;

namespace DigitalLove.Game.Spaceships
{
    public class BezierRay : MonoBehaviour
    {
        private const int Resolution = 50;

        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float secsToSelect;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float lineDistance = 1;
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private PlanetBehaviour originPlanet;

        private float countdown;
        private PlanetBehaviour destinationPlanet;

        private Vector3 LineDefaultDestinationPosition => lineRenderer.transform.position + lineRenderer.transform.forward * lineDistance;
        public Vector3[] GetSplinePositions() => splineContainer.GetPositions(Resolution);

        public Action<PlanetBehaviour> planetFound = (planet) => { };

        public void SetOriginPlanet(PlanetBehaviour originPlanet) => this.originPlanet = originPlanet;

        private void OnEnable()
        {
            ShowLineRenderer();
        }

        private void OnDisable()
        {
            if (destinationPlanet != null)
                destinationPlanet.SetIsDestination(false);
        }

        private void Update()
        {
            CheckHits();
            ShowLineRenderer();
        }

        private void CheckHits()
        {
            if (destinationPlanet != null && destinationPlanet.IsDestination)
                return;
            RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, 100, layerMask);
            if (hits.Length <= 0)
            {
                if (destinationPlanet != null)
                    destinationPlanet.SetIsDestination(false);
                destinationPlanet = null;
            }
            else
            {
                PlanetBehaviour hitPlanet = hits[0].rigidbody.GetComponent<PlanetBehaviour>();
                if (hitPlanet != null && hitPlanet != destinationPlanet && hitPlanet != originPlanet)
                {
                    if (destinationPlanet != null)
                        destinationPlanet.SetIsDestination(false);
                    destinationPlanet = hitPlanet;
                    countdown = secsToSelect;
                }
                else if (destinationPlanet != null)
                {
                    countdown -= Time.deltaTime;
                    if (countdown <= 0)
                    {
                        planetFound.Invoke(destinationPlanet);
                        destinationPlanet.SetIsDestination(true);
                    }
                }
            }
        }

        private void ShowLineRenderer()
        {
            if (destinationPlanet == null || !destinationPlanet.IsDestination)
            {
                Vector3 lineDestination = destinationPlanet != null ? destinationPlanet.transform.position : LineDefaultDestinationPosition;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPositions(new Vector3[] { transform.position, lineDestination });
            }
            else
            {
                UpdateSpline();
                lineRenderer.MatchSpline(splineContainer, Resolution);
            }
        }

        private void UpdateSpline()
        {

            Vector3 direction = (destinationPlanet.transform.position - originPlanet.transform.position).normalized;

            // Vector3 originPosition = splineContainer.transform.InverseTransformPoint(originPlanet.transform.position - direction * originPlanet.RadiusOffset);
            splineContainer.transform.position = originPlanet.transform.position - direction * originPlanet.RadiusOffset;

            splineContainer.transform.forward = direction;

            Vector3 destinationPosition = destinationPlanet.transform.position + direction * destinationPlanet.RadiusOffset;
            splineContainer.SetKnotPosition(2, splineContainer.transform.InverseTransformPoint(destinationPosition));

            Vector3 middlePosition = (splineContainer.transform.position + destinationPosition) / 2;

            float biggerRadius = destinationPlanet.RadiusOffset > originPlanet.RadiusOffset ? destinationPlanet.RadiusOffset : originPlanet.RadiusOffset;
            Vector3 midRight = splineContainer.transform.InverseTransformPoint(middlePosition + splineContainer.transform.right.normalized * biggerRadius);
            splineContainer.SetKnotPosition(1, midRight);
            Vector3 midLeft = splineContainer.transform.InverseTransformPoint(middlePosition - splineContainer.transform.right.normalized * biggerRadius);
            splineContainer.SetKnotPosition(3, midLeft);
        }

        [Header("Debug")]
        [SerializeField] private PlanetBehaviour toSet;

        [Button]
        private void SetDestinationPlanet()
        {
            destinationPlanet = toSet;
            destinationPlanet.SetIsDestination(true);
        }
    }
}