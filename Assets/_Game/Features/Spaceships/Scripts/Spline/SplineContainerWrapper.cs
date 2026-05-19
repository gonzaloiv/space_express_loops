using System;
using System.Collections.Generic;
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
        private TravellerRoutePlan travellerRoutePlan;

        public Vector3[] Positions => positions;
        public TravellerRoutePlan TravellerRoutePlan => travellerRoutePlan;

        private SplineContainer SplineContainer => splineContainer ??= GetComponent<SplineContainer>();

        public void SetColor(Color color)
        {
            lineRenderer.material.color = color;
        }

        public void BuildLoop(HubBehaviour hub, IReadOnlyList<PlanetBehaviour> destinations)
        {
            if (hub == null || destinations == null || destinations.Count == 0)
            {
                positions = null;
                travellerRoutePlan = null;
                return;
            }

            float hubRadius = hub.PlanetBody.Radius;
            Vector3 hubPosition = hub.transform.position;
            Vector3 hubForward = hub.transform.forward;

            List<Vector3> merged = new();
            List<Vector3[]> segments = new();
            List<PlanetBehaviour> pickupPlanets = new();

            Vector3 legOriginPosition = hubPosition;
            float legOriginRadius = hubRadius;
            Vector3 legOriginForward = hubForward;

            for (int i = 0; i < destinations.Count; i++)
            {
                PlanetBehaviour destination = destinations[i];
                Vector3 destPosition = destination.Position;
                float destRadius = destination.PlanetBody.Radius;

                Vector3[] leg = SampleLegPositions(
                    legOriginPosition, legOriginRadius, legOriginForward,
                    destPosition, destRadius);
                Vector3[] outbound = GetFirstHalf(leg);
                AppendPath(merged, outbound);
                segments.Add(outbound);
                pickupPlanets.Add(destination);

                legOriginPosition = destPosition;
                legOriginRadius = destRadius;
                legOriginForward = destination.transform.forward;
            }

            PlanetBehaviour last = destinations[destinations.Count - 1];
            Vector3[] returnLeg = SampleLegPositions(
                last.Position, last.PlanetBody.Radius, last.transform.forward,
                hubPosition, hubRadius);
            Vector3[] inbound = GetSecondHalf(returnLeg);
            AppendPath(merged, inbound);
            segments.Add(inbound);

            positions = merged.ToArray();
            travellerRoutePlan = new TravellerRoutePlan(segments, pickupPlanets);
        }

        public void SetLineRendererActive(bool isVisible)
        {
            lineRenderer.enabled = isVisible;
            if (isVisible && positions != null)
                lineRenderer.SetSplinePositions(positions);
            else
                lineRenderer.positionCount = 0;
        }

        private static void AppendPath(List<Vector3> merged, Vector3[] segment)
        {
            if (segment == null || segment.Length == 0)
                return;

            int startIndex = merged.Count > 0 ? 1 : 0;
            for (int i = startIndex; i < segment.Length; i++)
                merged.Add(segment[i]);
        }

        private Vector3[] SampleLegPositions(
            Vector3 originPosition,
            float originRadius,
            Vector3 originForward,
            Vector3 destinationPosition,
            float destinationRadius)
        {
            Vector3 direction = (destinationPosition - originPosition).normalized;
            if (direction.sqrMagnitude < 0.0001f)
                direction = originForward;

            SplineContainer.transform.position = originPosition - direction * originRadius * 1.25f;
            SplineContainer.transform.forward = direction;

            Vector3 destinationApproach = destinationPosition + direction * destinationRadius * 1.25f;
            SplineContainer.SetKnotPosition(3, SplineContainer.transform.InverseTransformPoint(destinationApproach));

            Vector3 oneThirdPosition = SplineContainer.transform.position + direction * originRadius * 1.25f;
            Vector3 originOffset = SplineContainer.transform.right.normalized * originRadius;

            SplineContainer.SetKnotPosition(1, SplineContainer.transform.InverseTransformPoint(oneThirdPosition + originOffset));
            SplineContainer.SetKnotPosition(5, SplineContainer.transform.InverseTransformPoint(oneThirdPosition - originOffset));

            Vector3 twoThirdsPosition = destinationApproach - direction * destinationRadius * 1.25f;
            Vector3 destinationOffset = SplineContainer.transform.right.normalized * destinationRadius;

            SplineContainer.SetKnotPosition(4, SplineContainer.transform.InverseTransformPoint(twoThirdsPosition + destinationOffset));
            SplineContainer.SetKnotPosition(2, SplineContainer.transform.InverseTransformPoint(twoThirdsPosition - destinationOffset));

            return SplineContainer.GetPositions(resolution);
        }

        private static Vector3[] GetFirstHalf(Vector3[] legPositions)
        {
            int halfLength = legPositions.Length / 2;
            Vector3[] half = new Vector3[halfLength];
            Array.Copy(legPositions, 0, half, 0, halfLength);
            return half;
        }

        private static Vector3[] GetSecondHalf(Vector3[] legPositions)
        {
            int halfLength = legPositions.Length / 2;
            Vector3[] half = new Vector3[halfLength];
            Array.Copy(legPositions, halfLength, half, 0, halfLength);
            return half;
        }
    }
}
