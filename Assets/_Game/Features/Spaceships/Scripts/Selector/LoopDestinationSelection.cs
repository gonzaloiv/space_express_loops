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
        private readonly GrabbableWrapper grabbableWrapper;
        private readonly DestinationSelector destinationSelector;
        private readonly SpaceshipRoute route;

        public LoopDestinationSelection(
            GrabbableWrapper grabbableWrapper,
            DestinationSelector destinationSelector,
            SpaceshipRoute route)
        {
            this.grabbableWrapper = grabbableWrapper;
            this.destinationSelector = destinationSelector;
            this.route = route;
        }

        public void Begin()
        {
            destinationSelector.SetSelectionOrigin(grabbableWrapper.Transform);
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

            if (!isFirstDestination)
                route.RebuildRoute();

            return isFirstDestination
                ? SelectionConfirmResult.StartedLoop
                : SelectionConfirmResult.ExtendedLoop;
        }
    }
}
