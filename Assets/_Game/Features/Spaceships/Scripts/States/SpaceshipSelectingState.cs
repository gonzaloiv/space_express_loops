using DigitalLove.FlowControl;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipSelectingState : State
    {
        private readonly StateMachine machine;
        private readonly SpaceshipRefs refs;
        private readonly SpaceshipDestinationFlow destinationFlow;
        private readonly ISpaceshipHost host;

        public SpaceshipSelectingState(
            StateMachine machine,
            SpaceshipRefs refs,
            SpaceshipDestinationFlow destinationFlow,
            ISpaceshipHost host)
        {
            this.machine = machine;
            this.refs = refs;
            this.destinationFlow = destinationFlow;
            this.host = host;
        }

        public void Init() => refs.ghost.SetActive(false);

        public override void Enter()
        {
            refs.grabbableWrapper.selected += OnSelect;
            refs.grabbableWrapper.released += OnRelease;
            destinationFlow.StartPicking();
        }

        public override void Exit()
        {
            refs.grabbableWrapper.selected -= OnSelect;
            refs.grabbableWrapper.released -= OnRelease;
            destinationFlow.StopPicking();
        }

        private void OnSelect()
        {
            if (!refs.destinationSelector.IsLookingForDestination)
                destinationFlow.StartPicking();
        }

        private void OnRelease()
        {
            if (!refs.destinationSelector.IsLookingForDestination)
                return;

            if (destinationFlow.TryAppendSelectedDestination())
            {
                host.NotifyLoopChanged();
                machine.SetCurrentState<SpaceshipRunningState>();
                return;
            }

            destinationFlow.ShowAtStation();
        }

        public void Debug_SimulateGrabSelect() => OnSelect();

        public void Debug_SimulateGrabRelease() => OnRelease();

        public void Debug_ConfirmDestination() => OnRelease();
    }
}
