using System;
using DigitalLove.Game.Planets;

namespace DigitalLove.Game.Spaceships
{
    public enum SelectionConfirmResult
    {
        Rejected,
        StartedLoop,
        ExtendedLoop
    }

    public class LoopDestinationSelection
    {
        private readonly DestinationSelector destinationSelector;
        private readonly SpaceshipRoute route;

        public LoopDestinationSelection(DestinationSelector destinationSelector, SpaceshipRoute route)
        {
            this.destinationSelector = destinationSelector;
            this.route = route;
        }

        public void Begin()
        {
            destinationSelector.SetExcludedPlanetIds(route.GetExcludedPlanetIds());
            destinationSelector.StartLookingForDestination(true);
        }

        public void End() => destinationSelector.StartLookingForDestination(false);

        public SelectionConfirmResult Confirm(PlanetBehaviour planet, Action onRouteChanged)
        {
            bool isFirstDestination = !route.HasDestinations;
            if (!route.TryAppendDestination(planet))
                return SelectionConfirmResult.Rejected;

            onRouteChanged?.Invoke();

            return isFirstDestination
                ? SelectionConfirmResult.StartedLoop
                : SelectionConfirmResult.ExtendedLoop;
        }
    }
}
