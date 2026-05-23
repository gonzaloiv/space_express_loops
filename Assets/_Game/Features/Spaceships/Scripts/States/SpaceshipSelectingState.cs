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

        public void Init() => refs.ghost.SetActive(false);

        public override void Enter()
        {
            refs.grabbableWrapper.selected += OnSelect;
            refs.grabbableWrapper.released += OnRelease;
            destinationSelection.StartPicking();
        }

        public override void Exit()
        {
            refs.grabbableWrapper.selected -= OnSelect;
            refs.grabbableWrapper.released -= OnRelease;
            destinationSelection.StopPicking();
        }

        private void OnSelect()
        {
            if (!refs.destinationSelector.IsLookingForDestination)
                destinationSelection.StartPicking();
        }

        private void OnRelease()
        {
            switch (destinationSelection.TryConfirmOnRelease(host, restartTravellerLoop: false))
            {
                case SelectionReleaseResult.Ignored:
                    return;
                case SelectionReleaseResult.Committed:
                    machine.SetCurrentState<SpaceshipRunningState>();
                    return;
                case SelectionReleaseResult.StayAtStation:
                    destinationSelection.ShowAtStation();
                    break;
            }
        }

        public void Debug_SimulateGrabSelect() => OnSelect();

        public void Debug_SimulateGrabRelease() => OnRelease();

        public void Debug_ConfirmDestination() => OnRelease();
    }
}
