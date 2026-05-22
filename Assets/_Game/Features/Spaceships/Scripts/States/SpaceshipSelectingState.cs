using DigitalLove.FlowControl;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipSelectingState : State
    {
        private readonly StateMachine machine;
        private readonly SpaceshipRefs refs;
        private readonly SpaceshipRouteSession session;
        private readonly ISpaceshipHost host;

        public SpaceshipSelectingState(
            StateMachine machine,
            SpaceshipRefs refs,
            SpaceshipRouteSession session,
            ISpaceshipHost host)
        {
            this.machine = machine;
            this.refs = refs;
            this.session = session;
            this.host = host;
        }

        public void Init() => refs.ghost.SetActive(false);

        public override void Enter()
        {
            refs.grabbableWrapper.EnablePointerHandling();
            refs.grabbableWrapper.selected += OnSelect;
            refs.grabbableWrapper.released += OnRelease;
            BeginSelecting();
        }

        public override void Exit()
        {
            refs.grabbableWrapper.selected -= OnSelect;
            refs.grabbableWrapper.released -= OnRelease;
            refs.ghost.SetActive(false);
            refs.destinationSelector.StartLookingForDestination(false);
        }

        private void BeginSelecting()
        {
            refs.ghost.SetActive(true);
            refs.grabbableWrapper.SetInteractionActive(true);
            refs.destinationSelector.SetExcludedPlanetIds(session.Route.GetExcludedPlanetIds());
            refs.destinationSelector.StartLookingForDestination(true);
        }

        private void OnSelect()
        {
            if (!refs.destinationSelector.IsLookingForDestination)
                BeginSelecting();
        }

        private void OnRelease()
        {
            if (refs.destinationSelector.IsLookingForDestination)
                OnDestinationSelection();
        }

        private void OnDestinationSelection()
        {
            bool hasAppended = session.Route.TryAppendDestination(refs.destinationSelector.Destination);
            if (hasAppended)
            {
                host.NotifyLoopChanged();
                machine.SetCurrentState<SpaceshipRunningState>();
                return;
            }

            ShowGrabbable();
        }

        private void ShowGrabbable()
        {
            refs.ghost.SetActive(false);
            refs.destinationSelector.StartLookingForDestination(false);
            refs.routePanel.SetButtonActive(true);
            refs.grabbableWrapper.Show();
            MoveShipToActiveStation();
        }

        private void MoveShipToActiveStation()
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

        public void Debug_SimulateGrabSelect() => OnSelect();

        public void Debug_SimulateGrabRelease() => OnRelease();

        public void Debug_ConfirmDestination() => OnDestinationSelection();
    }
}
