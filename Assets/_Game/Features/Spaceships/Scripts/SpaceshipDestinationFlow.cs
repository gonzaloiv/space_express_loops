namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipDestinationFlow
    {
        private readonly SpaceshipRefs refs;
        private readonly SpaceshipRouteSession session;

        public SpaceshipDestinationFlow(SpaceshipRefs refs, SpaceshipRouteSession session)
        {
            this.refs = refs;
            this.session = session;
        }

        public void StartPicking()
        {
            refs.ghost.SetActive(true);
            refs.grabbableWrapper.BeginDestinationSelection();
            refs.grabbableWrapper.SetInteractionActive(true);
            refs.destinationSelector.SetExcludedPlanetIds(session.Route.GetExcludedPlanetIds());
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
            refs.routePanel.SetButtonActive(true);
            refs.grabbableWrapper.Show();
            MoveToActiveStation();
        }

        public bool TryAppendSelectedDestination() =>
            session.Route.TryAppendDestination(refs.destinationSelector.Destination);

        public void MoveToActiveStation()
        {
            if (session.Route.IsLastLegToHub && session.Route.HasMoreThanOneDestination)
            {
                refs.grabbableWrapper.Hide();
                return;
            }

            refs.grabbableWrapper.Show();
            refs.grabbableWrapper.SetWorldPosition(
                session.Route.HasDestinations && session.Route.Destinations.Count > 1
                    ? session.Route.LastLegEndPosition
                    : session.Route.FirstLegEndPosition);
        }
    }
}
