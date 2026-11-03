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
        private TwoHandYawGesture yaw;

        public override void Enter()
        {
            scaleOut = new ScaleOutGesture(
                gesture.ScaleOutMinDistance, gesture.ScaleOutMaxRatio);
            yaw = new TwoHandYawGesture(
                gesture.RotationMinSeparation, gesture.RotationMaxVerticalRatio);
            gesture.StateChanged += OnGestureChanged;
        }

        public override void Exit()
        {
            gesture.StateChanged -= OnGestureChanged;
            scaleOut.Reset();
            yaw.Reset();
        }

        private void OnGestureChanged(TwoHandGestureState state)
        {
            if (scaleOut.Update(state))
            {
                ExitToSelection();
                return;
            }

            float delta = yaw.Update(state);
            if (!Mathf.Approximately(delta, 0f))
                puzzleController.Rotate(delta);
        }

        private void ExitToSelection()
        {
            puzzleController.Exit();
            puzzleController.HideAll();
            parent.SetCurrentState(gameStartState.RouteId);
        }
    }
}
