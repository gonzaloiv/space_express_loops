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
            refs.Ghost.SetActive(true);
            refs.GrabbableWrapper.Show(pickingDestination: true);
            refs.DestinationSelector.SetExcludedPlanetIds(session.GetDestinationIds());
            refs.DestinationSelector.StartLookingForDestination(true);
        }

        public void StopPicking()
        {
            refs.Ghost.SetActive(false);
            refs.DestinationSelector.StartLookingForDestination(false);
        }

        public void ShowAtStation()
        {
            StopPicking();
            MoveToActiveStation();
        }

        public SelectionReleaseResult TryConfirmOnRelease(ISpaceshipHost host, bool restartTravellerLoop)
        {
            if (!refs.DestinationSelector.IsLookingForDestination)
                return SelectionReleaseResult.Ignored;

            if (!session.TryCommitDestination(refs.DestinationSelector.Destination, host, restartTravellerLoop))
                return SelectionReleaseResult.StayAtStation;

            host.NotifyLoopChanged();
            return SelectionReleaseResult.Committed;
        }

        public void MoveToActiveStation()
        {
            refs.DestinationSelector.SetExcludedPlanetIds(session.GetDestinationIds());

            if (IsRouteCompleteAtHub())
            {
                refs.GrabbableWrapper.Hide();
                return;
            }

            refs.GrabbableWrapper.Show();
            refs.GrabbableWrapper.SetWorldPosition(session.GetGrabbableStationPosition());
            refs.GrabZone.PointAt(session.GetGrabbableLookTarget());
        }

        private bool IsRouteCompleteAtHub() =>
            session.HasDestinations
            && session.IsLastLegToHub
            && !hasSelectablePlanetsOffRoute();
    }
}