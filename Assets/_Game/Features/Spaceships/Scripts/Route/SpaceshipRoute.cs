using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipRoute
    {
        private readonly SplineContainerWrapper routeContainer;
        private readonly System.Func<HubBehaviour> getHub;
        private readonly List<PlanetBehaviour> destinations = new();

        public IReadOnlyList<PlanetBehaviour> Destinations => destinations;
        public bool HasDestinations => destinations.Count > 0;
        public bool HasMoreThanOneDestination => destinations.Count > 1;
        public HubBehaviour Hub => getHub();

        public Vector3 LastLegEndPosition => routeContainer.LastLegEndPosition;
        public Vector3 FirstLegEndPosition => routeContainer.FirstLegEndPosition;
        public bool IsLastLegToHub => routeContainer.LastLeg.pickupPlanet == null;

        public SpaceshipRoute(SplineContainerWrapper routeContainer, System.Func<HubBehaviour> getHub)
        {
            this.routeContainer = routeContainer;
            this.getHub = getHub;
        }

        public void SetColor(Color color) => routeContainer.SetColor(color);

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
            if (planets == null || planets.Count == 0)
                return;

            foreach (PlanetBehaviour planet in planets)
                destinations.Add(planet);
        }

        public void ClearDestinations() => destinations.Clear();

        public void RebuildRoute()
        {
            routeContainer.Build(Hub, destinations);
            routeContainer.SetLineRendererActive(destinations.Count > 0);
        }

        public List<string> GetDestinationIds()
        {
            List<string> ids = new(destinations.Count);
            foreach (PlanetBehaviour planet in destinations)
                ids.Add(planet.Id);
            return ids;
        }
    }
}
