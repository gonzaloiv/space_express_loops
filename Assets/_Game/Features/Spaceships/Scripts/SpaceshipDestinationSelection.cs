namespace DigitalLove.Game.Spaceships
{
    public enum SelectionReleaseResult
    {
        Ignored,
        StayAtStation,
        Committed
    }

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
            refs.grabbableWrapper.Show(pickingDestination: true);
            refs.destinationSelector.SetExcludedPlanetIds(session.GetDestinationIds());
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
            MoveToActiveStation();
        }

        public SelectionReleaseResult TryConfirmOnRelease(ISpaceshipHost host, bool restartTravellerLoop)
        {
            if (!refs.destinationSelector.IsLookingForDestination)
                return SelectionReleaseResult.Ignored;

            if (!session.TryCommitDestination(refs.destinationSelector.Destination, host, restartTravellerLoop))
                return SelectionReleaseResult.StayAtStation;

            host.NotifyLoopChanged();
            return SelectionReleaseResult.Committed;
        }

        public void MoveToActiveStation()
        {
            refs.destinationSelector.SetExcludedPlanetIds(session.GetDestinationIds());

            if (IsRouteCompleteAtHub())
            {
                refs.grabbableWrapper.Hide();
                return;
            }

            refs.grabbableWrapper.Show();
            refs.grabbableWrapper.SetWorldPosition(session.GetGrabbableStationPosition());
            refs.grabZone.PointAt(session.GetGrabbableLookTarget());
        }

        private bool IsRouteCompleteAtHub() =>
            session.HasDestinations
            && session.IsLastLegToHub
            && !hasSelectablePlanetsOffRoute();
    }
}