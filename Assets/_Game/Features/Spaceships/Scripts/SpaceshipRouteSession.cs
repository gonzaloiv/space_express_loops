using System;
using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipRouteSession
    {
        private readonly SplineContainerWrapper routeContainer;
        private readonly DestinationSelector destinationSelector;
        private readonly List<PlanetBehaviour> destinations = new();

        public TravellerRouteRunner TravellerLoop { get; }
        public IReadOnlyList<PlanetBehaviour> Destinations => destinations;
        public bool HasDestinations => destinations.Count > 0;

        public Vector3 LastLegEndPosition => routeContainer.LastLegEndPosition;
        public Vector3 FirstLegEndPosition => routeContainer.FirstLegEndPosition;
        public bool IsLastLegToHub =>
            routeContainer.HasLegs && routeContainer.LastLeg.pickupPlanet == null;

        public HubBehaviour Hub => destinationSelector.Hub;

        public SpaceshipRouteSession(SpaceshipRefs refs, MonoBehaviour coroutineHost)
        {
            routeContainer = refs.splineContainerWrapper;
            destinationSelector = refs.destinationSelector;
            TravellerLoop = new TravellerRouteRunner(
                coroutineHost,
                routeContainer,
                refs.traveller,
                refs.legDelay);
        }

        public void ResetVisuals()
        {
            TravellerLoop.Stop();
            SetLineRendererActive(false);
        }

        public void SetOnLoopComplete(Action<LoopCompleteEventArgs> onLoopComplete) =>
            TravellerLoop.SetOnLoopIterationComplete(onLoopComplete);

        public void SetRouteColor(Color color) => routeContainer.SetColor(color);

        public void SetLineRendererActive(bool active) => routeContainer.SetLineRendererActive(active);

        public IEnumerable<string> GetExcludedPlanetIds()
        {
            foreach (PlanetBehaviour planet in destinations)
                yield return planet.Id;
        }

        public bool TryAppendDestination(PlanetBehaviour planet)
        {
            if (planet == null || destinations.Contains(planet))
                return false;

            destinations.Add(planet);
            return true;
        }

        public void SetDestinations(IReadOnlyList<PlanetBehaviour> planets)
        {
            destinations.Clear();
            if (planets != null && planets.Count > 0)
                destinations.AddRange(planets);
        }

        public void ClearDestinations() => destinations.Clear();

        public void RebuildRoute()
        {
            routeContainer.Build(destinationSelector.Hub, destinations);
            SetLineRendererActive(HasDestinations);
        }

        public List<string> GetDestinationIds()
        {
            List<string> ids = new(destinations.Count);
            foreach (PlanetBehaviour planet in destinations)
                ids.Add(planet.Id);
            return ids;
        }

        public Vector3 GetStationCenter()
        {
            if (!HasDestinations)
                return Hub.Position;

            if (Destinations.Count == 1)
                return Destinations[0].Position;

            return IsLastLegToHub ? Hub.Position : Destinations[^1].Position;
        }
    }
}
