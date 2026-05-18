using System;
using DigitalLove.Game.Planets;
using UnityEngine;
using UnityEngine.Splines;

namespace DigitalLove.Game.Spaceships
{
    public class SplineContainerWrapper : MonoBehaviour
    {
        [SerializeField] private int resolution = 50;
        [SerializeField] private LineRenderer lineRenderer;

        private SplineContainer splineContainer;
        private Vector3[] positions;
        private Vector3[] goPositions;
        private Vector3[] returnPositions;

        public Vector3[] Positions => positions;
        public Vector3[] GoPositions => goPositions;
        public Vector3[] ReturnPositions => returnPositions;
        public Vector3 OriginPosition => positions[0];

        private SplineContainer SplineContainer => splineContainer ??= GetComponent<SplineContainer>();

        public void SetColor(Color color)
        {
            lineRenderer.material.color = color;
        }

        public void CreateLoop(HubBehaviour basePlanet, PlanetBehaviour destinationPlanet)
        {
            Vector3 direction = (destinationPlanet.transform.position - basePlanet.transform.position).normalized;
            float baseRadiusOffset = basePlanet.PlanetBody.Radius;

            SplineContainer.transform.position = basePlanet.transform.position - direction * baseRadiusOffset * 1.25f;
            SplineContainer.transform.forward = direction;

            Vector3 destinationPosition = destinationPlanet.transform.position + direction * destinationPlanet.PlanetBody.Radius * 1.25f;
            SplineContainer.SetKnotPosition(3, SplineContainer.transform.InverseTransformPoint(destinationPosition));

            Vector3 oneThirdPosition = SplineContainer.transform.position + direction * baseRadiusOffset * 1.25f;
            Vector3 originOffset = SplineContainer.transform.right.normalized * baseRadiusOffset;

            Vector3 oneThirdRight = SplineContainer.transform.InverseTransformPoint(oneThirdPosition + originOffset);
            SplineContainer.SetKnotPosition(1, oneThirdRight);
            Vector3 oneThirdLeft = SplineContainer.transform.InverseTransformPoint(oneThirdPosition - originOffset);
            SplineContainer.SetKnotPosition(5, oneThirdLeft);

            float destinationRadiusOffset = destinationPlanet.PlanetBody.Radius;
            Vector3 twoThirdsPosition = destinationPosition - direction * destinationRadiusOffset * 1.25f;
            Vector3 destinationOffset = SplineContainer.transform.right.normalized * destinationRadiusOffset;

            // ? Inverted for loop 
            Vector3 twoThirdsRight = SplineContainer.transform.InverseTransformPoint(twoThirdsPosition + destinationOffset);
            SplineContainer.SetKnotPosition(4, twoThirdsRight);

            Vector3 twoThirdsLeft = SplineContainer.transform.InverseTransformPoint(twoThirdsPosition - destinationOffset);
            SplineContainer.SetKnotPosition(2, twoThirdsLeft);

            positions = SplineContainer.GetPositions(resolution);
            CacheLegPositions();
        }

        private void CacheLegPositions()
        {
            int legLength = resolution / 2;
            goPositions = new Vector3[legLength];
            returnPositions = new Vector3[legLength];
            Array.Copy(positions, 0, goPositions, 0, legLength);
            Array.Copy(positions, legLength, returnPositions, 0, legLength);
        }

        public void SetLineRendererActive(bool isVisible)
        {
            lineRenderer.enabled = isVisible;
            if (isVisible && positions != null)
            {

                lineRenderer.SetSplinePositions(positions);
                lineRenderer.enabled = true;
            }
            else
            {
                lineRenderer.positionCount = 0;
                lineRenderer.enabled = false;
            }
        }
    }
}