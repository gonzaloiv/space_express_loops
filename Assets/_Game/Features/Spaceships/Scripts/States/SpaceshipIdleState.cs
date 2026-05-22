using DigitalLove.FlowControl;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipIdleState : State
    {
        private readonly StateMachine machine;
        private readonly SpaceshipRefs refs;
        private bool hasBeenGrabMePanelShown;

        public SpaceshipIdleState(StateMachine machine, SpaceshipRefs refs)
        {
            this.machine = machine;
            this.refs = refs;
        }

        public override void Enter()
        {
            refs.grabMePanel.SetActive(false);
            refs.grabbableWrapper.EnablePointerHandling();
            refs.grabbableWrapper.selected += OnSelect;
            refs.grabbableWrapper.Show();
            refs.destinationSelector.StartLookingForDestination(false);
            ShowGrabMePanel();
        }

        private void ShowGrabMePanel()
        {
            if (hasBeenGrabMePanelShown)
                return;

            refs.grabMePanel.SetActive(true);
            hasBeenGrabMePanelShown = true;
        }

        public override void Exit()
        {
            refs.grabbableWrapper.selected -= OnSelect;
            refs.grabbableWrapper.DisablePointerHandling();
        }

        private void OnSelect()
        {
            refs.grabMePanel.SetActive(false);
            machine.SetCurrentState<SpaceshipSelectingState>();
        }

        public void Debug_SimulateGrabSelect() => OnSelect();
    }
}
