using System.Collections.Generic;
using DigitalLove.Game.Planets;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class SinglePlanetRouteBuilder : IRouteLoopBuilder
    {
        private const float SplitT = 0.5f;

        private readonly SplineLegSampler sampler;

        public SinglePlanetRouteBuilder(SplineLegSampler sampler)
        {
            this.sampler = sampler;
        }

        public void Build(HubBehaviour hub, IReadOnlyList<PlanetBehaviour> destinations, List<RouteLegPath> legs)
        {
            PlanetBehaviour planet = destinations[0];
            sampler.SampleHalves(
                hub.transform.position, hub.PlanetBody.Radius, hub.transform.forward,
                planet.Position, planet.PlanetBody.Radius,
                SplitT, SplitT,
                out Vector3[] outbound, out Vector3[] inbound);

            legs.Add(new RouteLegPath(outbound, planet));
            legs.Add(new RouteLegPath(RoutePathUtility.CloseToLoopStart(inbound, outbound[0]), null));
        }
    }
}
