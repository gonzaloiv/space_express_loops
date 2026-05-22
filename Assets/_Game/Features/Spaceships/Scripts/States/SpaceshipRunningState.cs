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
            refs.routePanel.SetButtonActive(true);
            refs.routePanel.editButtonClicked += OnEditButtonClick;
            refs.grabbableWrapper.EnablePointerHandling();
            refs.grabbableWrapper.selected += OnSelect;
            refs.grabbableWrapper.released += OnRelease;
            BeginLoop();
        }

        public override void Exit()
        {
            refs.routePanel.SetButtonActive(false);
            refs.routePanel.editButtonClicked -= OnEditButtonClick;
            refs.grabbableWrapper.selected -= OnSelect;
            refs.grabbableWrapper.released -= OnRelease;
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
            if (!refs.destinationSelector.IsLookingForDestination)
                machine.SetCurrentState<SpaceshipSelectingState>();
        }

        private void OnRelease()
        {
            if (!refs.destinationSelector.IsLookingForDestination)
                return;

            if (destinationSelection.TryAppendSelectedDestination())
            {
                session.RebuildRoute();
                session.TravellerLoop.StartLoop(host.Id, host.BuildLoopEventArgs);
                host.NotifyLoopChanged();
            }

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
            refs.grabbableWrapper.Hide();
            session.SetLineRendererActive(false);
            refs.destinationSelector.StartLookingForDestination(false);
        }

        public void Debug_SimulateGrabSelect() => OnSelect();

        public void Debug_SimulateGrabRelease() => OnRelease();

        public void Debug_ConfirmDestination() => OnRelease();

        public void Debug_InvokeOnLoopEditionButtonClicked() => OnEditButtonClick();
    }
}
