using DigitalLove.FlowControl;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipSelectingState : State
    {
        private readonly StateMachine machine;
        private readonly SpaceshipRefs refs;
        private readonly SpaceshipDestinationSelection destinationSelection;
        private readonly ISpaceshipHost host;

        public SpaceshipSelectingState(
            StateMachine machine,
            SpaceshipRefs refs,
            SpaceshipDestinationSelection destinationSelection,
            ISpaceshipHost host)
        {
            this.machine = machine;
            this.refs = refs;
            this.destinationSelection = destinationSelection;
            this.host = host;
        }

        public void Init() => refs.Ghost.SetActive(false);

        public override void Enter()
        {
            refs.GrabbableWrapper.selected += OnSelect;
            refs.GrabbableWrapper.released += OnRelease;
            destinationSelection.StartPicking();
        }

        public override void Exit()
        {
            refs.GrabbableWrapper.selected -= OnSelect;
            refs.GrabbableWrapper.released -= OnRelease;
            destinationSelection.StopPicking();
        }

        private void OnSelect()
        {
            if (!refs.DestinationSelector.IsLookingForDestination)
                destinationSelection.StartPicking();
        }

        private void OnRelease()
        {
            switch (destinationSelection.TryConfirmOnRelease(host, restartTravellerLoop: false))
            {
                case SelectionReleaseResult.Ignored:
                    destinationSelection.ShowAtStation();
                    if (host.HasRoute)
                        machine.SetCurrentState<SpaceshipRunningState>();
                    else
                        machine.SetCurrentState<SpaceshipIdleState>();
                    return;
                case SelectionReleaseResult.Committed:
                    machine.SetCurrentState<SpaceshipRunningState>();
                    return;
                case SelectionReleaseResult.StayAtStation:
                    destinationSelection.ShowAtStation();
                    if (host.HasRoute)
                        machine.SetCurrentState<SpaceshipRunningState>();
                    else
                        machine.SetCurrentState<SpaceshipIdleState>();
                    break;
            }
        }

        public void Debug_SimulateGrabSelect() => OnSelect();

        public void Debug_SimulateGrabRelease() => OnRelease();
    }
}
