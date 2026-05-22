using DigitalLove.FlowControl;
using DigitalLove.Global;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipRunningState : State
    {
        private readonly StateMachine machine;
        private readonly SpaceshipRefs refs;
        private readonly SpaceshipRouteSession session;
        private readonly ISpaceshipHost host;

        public SpaceshipRunningState(
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
            ShowGrabbable();
            session.TravellerLoop.StartLoop(host.Id, host.BuildLoopEventArgs);
        }

        private void OnSelect()
        {
            if (!refs.destinationSelector.IsLookingForDestination)
                machine.SetCurrentState<SpaceshipSelectingState>();
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
                session.Route.RebuildRoute();
                session.TravellerLoop.StartLoop(host.Id, host.BuildLoopEventArgs);
                host.NotifyLoopChanged();
            }

            ShowGrabbable();
        }

        private void OnEditButtonClick()
        {
            session.Route.ClearDestinations();
            host.NotifyLoopEditionClicked();
            machine.SetCurrentState<SpaceshipIdleState>();
        }

        private void HideVisuals()
        {
            refs.ghost.SetActive(false);
            refs.grabbableWrapper.Hide();
            refs.routePanel.Hide();
            session.Route.SetLineRendererActive(false);
            refs.destinationSelector.StartLookingForDestination(false);
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

        public void Debug_InvokeOnLoopEditionButtonClicked() => OnEditButtonClick();
    }
}
