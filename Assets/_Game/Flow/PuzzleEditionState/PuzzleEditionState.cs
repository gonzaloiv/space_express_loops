using DigitalLove.FlowControl;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Input;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class PuzzleEditionState : MonoState
    {
        [SerializeField] private MonoState gameStartState;
        [SerializeField] private TwoHandGestureInput gesture;
        [SerializeField] private SpaceShapingController puzzleController;

        private ScaleOutGesture scaleOut;

        public override void Enter()
        {
            scaleOut = new ScaleOutGesture(
                gesture.ScaleOutMinDistance, gesture.ScaleOutMaxRatio);
            gesture.StateChanged += OnGestureChanged;
        }

        public override void Exit()
        {
            gesture.StateChanged -= OnGestureChanged;
            scaleOut.Reset();
        }

        private void OnGestureChanged(TwoHandGestureState state)
        {
            if (!scaleOut.Update(state))
                return;

            puzzleController.Exit();
            puzzleController.HideAll();
            parent.SetCurrentState(gameStartState.RouteId);
        }
    }
}
