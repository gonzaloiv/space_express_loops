using System.Collections.Generic;
using DigitalLove.Game.Planets;
using UnityEngine;
using UnityEngine.Splines;

namespace DigitalLove.Game.Spaceships
{
    public class SplineContainerWrapper : MonoBehaviour
    {
        private const float OutboundEndT = 0.5f;
        private const float InboundStartT = 0.5f;

        [SerializeField] private int samplesPerHalf = 32;
        [SerializeField] private LineRenderer lineRenderer;

        private SplineContainer splineContainer;
        private readonly List<RouteLegPath> legs = new();
        private TravellerRoutePlan travellerRoutePlan;

        public IReadOnlyList<RouteLegPath> Legs => legs;
        public TravellerRoutePlan TravellerRoutePlan => travellerRoutePlan;

        private SplineContainer SplineContainer => splineContainer ??= GetComponent<SplineContainer>();

        public void SetColor(Color color) => lineRenderer.material.color = color;

        public Vector3 GetPanelAnchorPosition()
        {
            if (legs.Count == 0 || legs[0].Positions == null || legs[0].Positions.Length == 0)
                return transform.position;

            Vector3[] firstLeg = legs[0].Positions;
            return firstLeg[firstLeg.Length / 2];
        }

        public void BuildLoop(HubBehaviour hub, IReadOnlyList<PlanetBehaviour> destinations)
        {
            legs.Clear();
            travellerRoutePlan = null;

            if (hub == null || destinations == null || destinations.Count == 0)
                return;

            if (destinations.Count == 1)
                BuildSinglePlanetLoop(hub, destinations[0]);
            else
                BuildMultiPlanetLoop(hub, destinations);

            travellerRoutePlan = CreateTravellerPlan();
            RefreshLineRenderer();
        }

        public void SetLineRendererActive(bool isVisible)
        {
            lineRenderer.enabled = isVisible;
            if (!isVisible)
                lineRenderer.positionCount = 0;
            else
                RefreshLineRenderer();
        }

        private void BuildSinglePlanetLoop(HubBehaviour hub, PlanetBehaviour planet)
        {
            SampleLegHalves(
                hub.transform.position, hub.PlanetBody.Radius, hub.transform.forward,
                planet.Position, planet.PlanetBody.Radius,
                out Vector3[] outbound, out Vector3[] inbound);

            legs.Add(new RouteLegPath(outbound, planet));
            legs.Add(new RouteLegPath(inbound, null));
        }

        private void BuildMultiPlanetLoop(HubBehaviour hub, IReadOnlyList<PlanetBehaviour> destinations)
        {
            PlanetBehaviour first = destinations[0];
            SampleLegHalves(
                hub.transform.position, hub.PlanetBody.Radius, hub.transform.forward,
                first.Position, first.PlanetBody.Radius,
                out Vector3[] outbound, out _);
            legs.Add(new RouteLegPath(outbound, first));

            for (int i = 1; i < destinations.Count; i++)
            {
                PlanetBehaviour from = destinations[i - 1];
                PlanetBehaviour to = destinations[i];
                Vector3[] legToNext = SampleLegHalf(
                    from.Position, from.PlanetBody.Radius, from.transform.forward,
                    to.Position, to.PlanetBody.Radius,
                    tStart: 0f, tEnd: OutboundEndT);
                legs.Add(new RouteLegPath(legToNext, to));
            }

            PlanetBehaviour last = destinations[destinations.Count - 1];
            Vector3[] returnToHub = SampleLegHalf(
                last.Position, last.PlanetBody.Radius, last.transform.forward,
                hub.transform.position, hub.PlanetBody.Radius,
                tStart: InboundStartT, tEnd: 1f);
            legs.Add(new RouteLegPath(returnToHub, null));
        }

        private void SampleLegHalves(
            Vector3 originPosition,
            float originRadius,
            Vector3 originForward,
            Vector3 destinationPosition,
            float destinationRadius,
            out Vector3[] outboundHalf,
            out Vector3[] inboundHalf)
        {
            ConfigureSplineForLeg(originPosition, originRadius, originForward, destinationPosition, destinationRadius);
            outboundHalf = SplineContainer.GetPositions(samplesPerHalf, 0f, OutboundEndT);
            inboundHalf = SplineContainer.GetPositions(samplesPerHalf, InboundStartT, 1f);
        }

        private Vector3[] SampleLegHalf(
            Vector3 originPosition,
            float originRadius,
            Vector3 originForward,
            Vector3 destinationPosition,
            float destinationRadius,
            float tStart,
            float tEnd)
        {
            ConfigureSplineForLeg(originPosition, originRadius, originForward, destinationPosition, destinationRadius);
            return SplineContainer.GetPositions(samplesPerHalf, tStart, tEnd);
        }

        private TravellerRoutePlan CreateTravellerPlan()
        {
            List<Vector3[]> segments = new(legs.Count);
            List<PlanetBehaviour> pickupPlanets = new();

            foreach (RouteLegPath leg in legs)
            {
                segments.Add(leg.Positions);
                if (leg.PickupPlanet != null)
                    pickupPlanets.Add(leg.PickupPlanet);
            }

            return new TravellerRoutePlan(segments, pickupPlanets);
        }

        private void RefreshLineRenderer()
        {
            List<Vector3> merged = new();
            foreach (RouteLegPath leg in legs)
                AppendPath(merged, leg.Positions);

            if (merged.Count == 0)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            lineRenderer.SetSplinePositions(merged.ToArray());
        }

        private static void AppendPath(List<Vector3> merged, Vector3[] segment)
        {
            if (segment == null || segment.Length == 0)
                return;

            int startIndex = merged.Count > 0 ? 1 : 0;
            for (int i = startIndex; i < segment.Length; i++)
                merged.Add(segment[i]);
        }

        private void ConfigureSplineForLeg(
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
        }
    }
}
