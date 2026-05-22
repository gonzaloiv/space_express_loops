using System;
using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipRouteSession
    {
        public SpaceshipRoute Route { get; }
        public TravellerLoopRunner TravellerLoop { get; }

        public bool HasDestinations => Route.HasDestinations;

        public SpaceshipRouteSession(SpaceshipRefs refs, MonoBehaviour coroutineHost)
        {
            Route = new SpaceshipRoute(refs.splineContainerWrapper, () => refs.destinationSelector.Hub);
            TravellerLoop = new TravellerLoopRunner(
                coroutineHost,
                refs.splineContainerWrapper,
                refs.traveller,
                refs.legDelay);
        }

        public void ResetVisuals()
        {
            TravellerLoop.Stop();
            Route.SetLineRendererActive(false);
        }

        public void SetOnLoopComplete(Action<LoopCompleteEventArgs> onLoopComplete) =>
            TravellerLoop.SetOnLoopIterationComplete(onLoopComplete);

        public void SetDestinations(IReadOnlyList<PlanetBehaviour> destinations) =>
            Route.SetDestinations(destinations);

        public void ClearDestinations() => Route.ClearDestinations();

        public void SetRouteColor(Color color) => Route.SetColor(color);

        public List<string> GetDestinationIds() => Route.GetDestinationIds();
    }
}
