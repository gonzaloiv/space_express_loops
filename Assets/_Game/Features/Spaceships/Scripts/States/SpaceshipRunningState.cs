using DigitalLove.FlowControl;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipRunningState : State
    {
        private readonly StateMachine machine;
        private readonly SpaceshipRefs refs;
        private readonly SpaceshipRouteSession session;
        private readonly SpaceshipDestinationFlow destinationFlow;
        private readonly ISpaceshipHost host;

        public SpaceshipRunningState(
            StateMachine machine,
            SpaceshipRefs refs,
            SpaceshipRouteSession session,
            SpaceshipDestinationFlow destinationFlow,
            ISpaceshipHost host)
        {
            this.machine = machine;
            this.refs = refs;
            this.session = session;
            this.destinationFlow = destinationFlow;
            this.host = host;
        }

        public override void Enter()
        {
            refs.routePanel.editButtonClicked += OnEditButtonClick;
            refs.grabbableWrapper.EnablePointerHandling();
            refs.grabbableWrapper.selected += OnSelect;
            refs.grabbableWrapper.released += OnRelease;
            BeginLoop();
        }

        public override void Exit()
        {
            refs.routePanel.editButtonClicked -= OnEditButtonClick;
            refs.grabbableWrapper.selected -= OnSelect;
            refs.grabbableWrapper.released -= OnRelease;
            session.TravellerLoop.Stop();
            HideVisuals();
        }

        private void BeginLoop()
        {
            session.Route.RebuildRoute();
            destinationFlow.ShowAtStation();
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

            if (destinationFlow.TryAppendSelectedDestination())
            {
                session.Route.RebuildRoute();
                session.TravellerLoop.StartLoop(host.Id, host.BuildLoopEventArgs);
                host.NotifyLoopChanged();
            }

            destinationFlow.ShowAtStation();
        }

        private void OnEditButtonClick()
        {
            session.Route.ClearDestinations();
            host.NotifyLoopEditionClicked();
            machine.SetCurrentState<SpaceshipIdleState>();
        }

        private void HideVisuals()
        {
            refs.grabbableWrapper.Hide();
            refs.routePanel.Hide();
            session.Route.SetLineRendererActive(false);
            refs.destinationSelector.StartLookingForDestination(false);
        }

        public void Debug_SimulateGrabSelect() => OnSelect();

        public void Debug_SimulateGrabRelease() => OnRelease();

        public void Debug_ConfirmDestination() => OnRelease();

        public void Debug_InvokeOnLoopEditionButtonClicked() => OnEditButtonClick();
    }
}
