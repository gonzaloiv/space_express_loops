namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipDestinationSelection
    {
        private readonly SpaceshipRefs refs;
        private readonly SpaceshipRouteSession session;
        private readonly System.Func<bool> hasSelectablePlanetsOffRoute;

        public SpaceshipDestinationSelection(
            SpaceshipRefs refs,
            SpaceshipRouteSession session,
            System.Func<bool> hasSelectablePlanetsOffRoute)
        {
            this.refs = refs;
            this.session = session;
            this.hasSelectablePlanetsOffRoute = hasSelectablePlanetsOffRoute;
        }

        public void StartPicking()
        {
            refs.ghost.SetActive(true);
            refs.grabbableWrapper.BeginDestinationSelection();
            refs.grabbableWrapper.SetInteractionActive(true);
            refs.destinationSelector.SetExcludedPlanetIds(session.GetExcludedPlanetIds());
            refs.destinationSelector.StartLookingForDestination(true);
        }

        public void StopPicking()
        {
            refs.ghost.SetActive(false);
            refs.destinationSelector.StartLookingForDestination(false);
        }

        public void ShowAtStation()
        {
            StopPicking();
            refs.grabbableWrapper.Show();
            MoveToActiveStation();
        }

        public bool TryAppendSelectedDestination() =>
            session.TryAppendDestination(refs.destinationSelector.Destination);

        public void MoveToActiveStation()
        {
            refs.destinationSelector.SetExcludedPlanetIds(session.GetExcludedPlanetIds());

            if (session.IsLastLegToHub && ShouldHideGrabbableAtStation())
            {
                refs.grabbableWrapper.Hide();
                return;
            }

            refs.grabbableWrapper.Show();
            refs.grabbableWrapper.SetWorldPosition(
                session.HasDestinations && session.Destinations.Count > 1
                    ? session.LastLegEndPosition
                    : session.FirstLegEndPosition);
            refs.grabZone.LookAtStationCenter(session.GetStationCenter());
        }

        private bool ShouldHideGrabbableAtStation() => !hasSelectablePlanetsOffRoute();
    }
}
