using UnityEngine;
using UnityEngine.Splines;
using DigitalLove.Game.Nodes;
using System.Collections.Generic;

namespace DigitalLove.Game.Spaceships
{
    public class SplineLegSampler
    {
        private const float PlanetClearance = 1.25f;

        private readonly SplineContainer splineContainer;
        private readonly int resolution;
        private readonly Transform contentRoot;

        public SplineLegSampler(SplineContainer splineContainer, int resolution = 32, Transform contentRoot = null)
        {
            this.splineContainer = splineContainer;
            this.resolution = resolution;
            this.contentRoot = contentRoot;
        }

        public void Build(HubBehaviour hub, IReadOnlyList<PlanetBehaviour> destinations, IList<RouteLeg> legs)
        {
            if (destinations.Count == 1)
            {
                GetSingleDestinationLeg(hub, destinations[0], legs);
                return;
            }

            CreateMultipleLegs(hub, destinations, legs);
        }

        private void CreateMultipleLegs(HubBehaviour hub, IReadOnlyList<PlanetBehaviour> destinations, IList<RouteLeg> legs)
        {
            ConfigureSplineForLeg(hub.NodeBody, destinations[0].NodeBody);
            legs.Add(CreateLeg(destinations[0]));

            for (int i = 1; i < destinations.Count; i++)
            {
                ConfigureSplineForLeg(destinations[i - 1].NodeBody, destinations[i].NodeBody);
                legs.Add(CreateLeg(destinations[i]));
            }

            ConfigureSplineForLeg(destinations[destinations.Count - 1].NodeBody, hub.NodeBody);
            legs.Add(CreateLeg(null));
        }

        private void GetSingleDestinationLeg(HubBehaviour hub, PlanetBehaviour destination, IList<RouteLeg> legs)
        {
            ConfigureSplineForLeg(hub.NodeBody, destination.NodeBody);
            legs.Add(CreateLeg(destination));
            ConfigureSplineForLeg(destination.NodeBody, hub.NodeBody, true);
            legs.Add(CreateLeg(null));
        }

        private RouteLeg CreateLeg(PlanetBehaviour pickupPlanet) =>
            new(splineContainer.GetPositions(Mathf.Max(2, resolution / 2), contentRoot, 0.5f), pickupPlanet);

        private void ConfigureSplineForLeg(NodeBody origin, NodeBody destination, bool inverse = false)
        {
            Vector3 direction = (destination.Position - origin.Position).normalized;
            splineContainer.transform.forward = direction;

            Vector3 originPosition = origin.Position - direction * origin.Radius * PlanetClearance;
            Vector3 destinationPosition = destination.Position + direction * destination.Radius * PlanetClearance;

            SetEndpointKnots(originPosition, destinationPosition);
            SetControlKnots(origin, destination, originPosition, destinationPosition, direction, inverse);
        }

        private void SetEndpointKnots(Vector3 originPosition, Vector3 destinationPosition)
        {
            splineContainer.SetKnotPosition(0, splineContainer.transform.InverseTransformPoint(originPosition));
            splineContainer.SetKnotPosition(6, splineContainer.transform.InverseTransformPoint(originPosition));
            splineContainer.SetKnotPosition(3, splineContainer.transform.InverseTransformPoint(destinationPosition));
        }

        private void SetControlKnots(
            NodeBody origin,
            NodeBody destination,
            Vector3 originPosition,
            Vector3 destinationPosition,
            Vector3 direction,
            bool inverse)
        {
            Vector3 lateralAxis = splineContainer.transform.right.normalized;
            if (inverse)
                lateralAxis = -lateralAxis;

            Vector3 originLateralOffset = lateralAxis * origin.Radius * PlanetClearance;
            Vector3 destinationLateralOffset = lateralAxis * destination.Radius * PlanetClearance;
            Vector3 oneThirdPosition = originPosition + direction * origin.Radius * PlanetClearance;
            Vector3 twoThirdsPosition = destinationPosition - direction * destination.Radius * PlanetClearance;

            splineContainer.SetKnotPosition(1, splineContainer.transform.InverseTransformPoint(oneThirdPosition + originLateralOffset));
            splineContainer.SetKnotPosition(2, splineContainer.transform.InverseTransformPoint(twoThirdsPosition - destinationLateralOffset));
            splineContainer.SetKnotPosition(4, splineContainer.transform.InverseTransformPoint(twoThirdsPosition + destinationLateralOffset));
            splineContainer.SetKnotPosition(5, splineContainer.transform.InverseTransformPoint(oneThirdPosition - originLateralOffset));
        }
    }
}
