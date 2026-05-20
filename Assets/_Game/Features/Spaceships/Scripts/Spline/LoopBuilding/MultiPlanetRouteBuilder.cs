using System.Collections.Generic;
using DigitalLove.Game.Planets;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class MultiPlanetRouteBuilder : IRouteLoopBuilder
    {
        private const float OutboundEndT = 0.5f;
        private const float InboundStartT = 0.5f;

        private readonly SplineLegSampler sampler;

        public MultiPlanetRouteBuilder(SplineLegSampler sampler)
        {
            this.sampler = sampler;
        }

        public void Build(HubBehaviour hub, IReadOnlyList<PlanetBehaviour> destinations, List<RouteLegPath> legs)
        {
            PlanetBehaviour first = destinations[0];
            sampler.SampleHalves(
                hub.transform.position, hub.PlanetBody.Radius, hub.transform.forward,
                first.Position, first.PlanetBody.Radius,
                OutboundEndT, InboundStartT,
                out Vector3[] outbound, out _);
            legs.Add(new RouteLegPath(outbound, first));

            for (int i = 1; i < destinations.Count; i++)
            {
                PlanetBehaviour from = destinations[i - 1];
                PlanetBehaviour to = destinations[i];
                bool isLastPlanet = i == destinations.Count - 1;
                Vector3[] legToNext = sampler.SampleRange(
                    from.Position, from.PlanetBody.Radius, from.transform.forward,
                    to.Position, to.PlanetBody.Radius,
                    isLastPlanet ? InboundStartT : 0f,
                    isLastPlanet ? 1f : OutboundEndT);
                legs.Add(new RouteLegPath(legToNext, to));
            }

            PlanetBehaviour last = destinations[destinations.Count - 1];
            Vector3[] returnToHub = sampler.SampleRange(
                last.Position, last.PlanetBody.Radius, last.transform.forward,
                hub.transform.position, hub.PlanetBody.Radius,
                InboundStartT, 1f);
            Vector3 previousLegEnd = legs[^1].Positions[^1];
            returnToHub[0] = previousLegEnd;
            legs.Add(new RouteLegPath(RoutePathUtility.CloseToLoopStart(returnToHub, legs[0].Positions[0]), null));
        }
    }
}
