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

            refs.GrabMePanel.SetActive(false);
        }

        public override void Enter()
        {
            refs.GrabMePanel.SetActive(false);
            refs.GrabbableWrapper.selected += OnSelect;
            refs.GrabbableWrapper.Show();
            refs.DestinationSelector.StartLookingForDestination(false);
            ShowGrabMePanel();
        }

        private void ShowGrabMePanel()
        {
            if (hasBeenGrabMePanelShown)
                return;

            refs.GrabMePanel.SetActive(true);
            hasBeenGrabMePanelShown = true;
        }

        public override void Exit()
        {
            refs.GrabbableWrapper.selected -= OnSelect;
            refs.GrabbableWrapper.DisablePointerHandling();
        }

        private void OnSelect()
        {
            refs.GrabMePanel.SetActive(false);
            machine.SetCurrentState<SpaceshipSelectingState>();
        }

        public void Debug_SimulateGrabSelect() => OnSelect();
    }
}
