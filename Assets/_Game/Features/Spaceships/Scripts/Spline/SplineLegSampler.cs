using UnityEngine;
using UnityEngine.Splines;
using DigitalLove.Game.Planets;
using System.Collections.Generic;

namespace DigitalLove.Game.Spaceships
{
    public class SplineLegSampler
    {
        private readonly SplineContainer splineContainer;
        private readonly int resolution;

        public SplineLegSampler(SplineContainer splineContainer, int resolution = 32)
        {
            this.splineContainer = splineContainer;
            this.resolution = resolution;
        }

        public void Build(HubBehaviour hub, IReadOnlyList<PlanetBehaviour> destinations, IList<RouteLeg> legs)
        {
            if (destinations.Count == 1)
                GetSingleDestinationLeg(hub, destinations[0], legs);
            else
                CreateMultipleLegs(hub, destinations, legs);
        }

        private void CreateMultipleLegs(HubBehaviour hub, IReadOnlyList<PlanetBehaviour> destinations, IList<RouteLeg> legs)
        {
            ConfigureSplineForLeg(hub.PlanetBody, destinations[0].PlanetBody);
            legs.Add(GetTruncatedLeg(splineContainer.GetPositions(resolution), destinations[0]));

            for (int i = 1; i < destinations.Count; i++)
            {
                ConfigureSplineForLeg(destinations[i - 1].PlanetBody, destinations[i].PlanetBody);
                legs.Add(GetTruncatedLeg(splineContainer.GetPositions(resolution), destinations[i]));
            }

            ConfigureSplineForLeg(destinations[destinations.Count - 1].PlanetBody, hub.PlanetBody);
            legs.Add(GetTruncatedLeg(splineContainer.GetPositions(resolution), null));
        }

        private RouteLeg GetTruncatedLeg(List<Vector3> positions, PlanetBehaviour pickupPlanet)
        {
            positions.RemoveRange(positions.Count / 2, positions.Count / 2);
            return new(positions, pickupPlanet);
        }

        private void GetSingleDestinationLeg(HubBehaviour hub, PlanetBehaviour destination, IList<RouteLeg> legs)
        {
            ConfigureSplineForLeg(hub.PlanetBody, destination.PlanetBody);
            legs.Add(new RouteLeg(splineContainer.GetPositions(resolution), destination));
        }

        private void ConfigureSplineForLeg(PlanetBody origin, PlanetBody destination)
        {
            Vector3 direction = (destination.Position - origin.Position).normalized;
            splineContainer.transform.forward = direction;

            Vector3 originPosition = origin.Position - direction * origin.Radius * 1.25f;
            splineContainer.SetKnotPosition(0, splineContainer.transform.InverseTransformPoint(originPosition));
            splineContainer.SetKnotPosition(6, splineContainer.transform.InverseTransformPoint(originPosition));

            Vector3 destinationPosition = destination.Position + direction * destination.Radius * 1.25f;
            splineContainer.SetKnotPosition(3, splineContainer.transform.InverseTransformPoint(destinationPosition));

            Vector3 lateralAxis = splineContainer.transform.right.normalized;
            Vector3 originLateralOffset = lateralAxis * origin.Radius * 1.25f;
            Vector3 destinationLateralOffset = lateralAxis * destination.Radius * 1.25f;
            Vector3 oneThirdPosition = originPosition + direction * origin.Radius * 1.25f;
            Vector3 twoThirdsPosition = destinationPosition - direction * destination.Radius * 1.25f;

            splineContainer.SetKnotPosition(1, splineContainer.transform.InverseTransformPoint(oneThirdPosition + originLateralOffset));
            splineContainer.SetKnotPosition(2, splineContainer.transform.InverseTransformPoint(twoThirdsPosition - destinationLateralOffset));

            splineContainer.SetKnotPosition(4, splineContainer.transform.InverseTransformPoint(twoThirdsPosition + destinationLateralOffset));
            splineContainer.SetKnotPosition(5, splineContainer.transform.InverseTransformPoint(oneThirdPosition - originLateralOffset));
        }
    }
}
