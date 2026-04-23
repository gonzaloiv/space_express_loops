using System;
using DigitalLove.Game.Planets;
using DigitalLove.Global;
using Oculus.Interaction;
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

        private PlanetBehaviour originPlanet;
        private Transform body;
        private float countdown;
        private PlanetBehaviour destinationPlanet;

        private Vector3 LineDefaultDestinationPosition => lineRenderer.transform.position + lineRenderer.transform.forward * lineDistance;
        public Vector3[] GetSplinePositions() => splineContainer.GetPositions(Resolution);

        public Action<PlanetBehaviour> planetFound = (planet) => { };

        public void Init(PlanetBehaviour originPlanet, Transform body)
        {
            this.originPlanet = originPlanet;
            this.body = body;
        }

        public void DeselectPlanet()
        {
            if (destinationPlanet != null)
                destinationPlanet.SetIsDestination(false);
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
            transform.SetPose(body.ToWorldPose());
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

            splineContainer.transform.position = originPlanet.transform.position - direction * originPlanet.RadiusOffset;
            splineContainer.transform.forward = direction;

            Vector3 destinationPosition = destinationPlanet.transform.position + direction * destinationPlanet.RadiusOffset;
            splineContainer.SetKnotPosition(3, splineContainer.transform.InverseTransformPoint(destinationPosition));

            float distance = Vector3.Distance(destinationPosition, splineContainer.transform.position);

            Vector3 oneThirdPosition = splineContainer.transform.position + direction * distance / 3;
            Vector3 originOffset = splineContainer.transform.right.normalized * originPlanet.RadiusOffset;

            Vector3 oneThirdRight = splineContainer.transform.InverseTransformPoint(oneThirdPosition + originOffset);
            splineContainer.SetKnotPosition(1, oneThirdRight);
            Vector3 oneThirdLeft = splineContainer.transform.InverseTransformPoint(oneThirdPosition - originOffset);
            splineContainer.SetKnotPosition(5, oneThirdLeft);

            Vector3 twoThirdsPosition = destinationPosition - direction * distance / 3;
            Vector3 destinationOffset = splineContainer.transform.right.normalized * destinationPlanet.RadiusOffset;

            Vector3 twoThirdsRight = splineContainer.transform.InverseTransformPoint(twoThirdsPosition + destinationOffset);
            splineContainer.SetKnotPosition(2, twoThirdsRight);
            Vector3 twoThirdsLeft = splineContainer.transform.InverseTransformPoint(twoThirdsPosition - destinationOffset);
            splineContainer.SetKnotPosition(4, twoThirdsLeft);
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