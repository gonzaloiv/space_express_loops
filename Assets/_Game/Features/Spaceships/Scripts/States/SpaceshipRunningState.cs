using DigitalLove.FlowControl;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipRunningState : State
    {
        private readonly StateMachine machine;
        private readonly SpaceshipRefs refs;
        private readonly SpaceshipRouteSession session;
        private readonly SpaceshipDestinationSelection destinationSelection;
        private readonly ISpaceshipHost host;

        public SpaceshipRunningState(
            StateMachine machine,
            SpaceshipRefs refs,
            SpaceshipRouteSession session,
            SpaceshipDestinationSelection destinationSelection,
            ISpaceshipHost host)
        {
            this.machine = machine;
            this.refs = refs;
            this.session = session;
            this.destinationSelection = destinationSelection;
            this.host = host;
        }

        public override void Enter()
        {
            refs.RoutePanel.editButtonClicked += OnEditButtonClick;
            refs.RoutePanel.SetButtonActive(true);
            refs.GrabbableWrapper.selected += OnSelect;
            refs.GrabbableWrapper.released += OnRelease;
            BeginLoop();
        }

        public override void Exit()
        {
            refs.RoutePanel.SetButtonActive(false);
            refs.RoutePanel.editButtonClicked -= OnEditButtonClick;
            refs.GrabbableWrapper.selected -= OnSelect;
            refs.GrabbableWrapper.released -= OnRelease;
            session.TravellerLoop.Stop();
            HideVisuals();
        }

        private void BeginLoop()
        {
            session.RebuildRoute();
            destinationSelection.ShowAtStation();
            session.TravellerLoop.StartLoop(host.Id, host.BuildLoopEventArgs);
        }

        private void OnSelect()
        {
            if (!refs.DestinationSelector.IsLookingForDestination)
                machine.SetCurrentState<SpaceshipSelectingState>();
        }

        private void OnRelease()
        {
            if (destinationSelection.TryConfirmOnRelease(host, restartTravellerLoop: true) == SelectionReleaseResult.Ignored)
                return;

            destinationSelection.ShowAtStation();
        }

        private void OnEditButtonClick()
        {
            session.ClearDestinations();
            host.NotifyLoopEditionClicked();
            machine.SetCurrentState<SpaceshipIdleState>();
        }

        private void HideVisuals()
        {
            refs.GrabbableWrapper.Hide();
            session.SetLineRendererActive(false);
            refs.DestinationSelector.StartLookingForDestination(false);
        }

        public void Debug_SimulateGrabSelect() => OnSelect();

        public void Debug_SimulateGrabRelease() => OnRelease();

        public void Debug_InvokeOnLoopEditionButtonClicked() => OnEditButtonClick();
    }
}
