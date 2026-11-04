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
        [SerializeField] private Transform head;
        [SerializeField] private bool rotationEnabled;
        [SerializeField] private float rotationStepDegrees = 15f;
        [SerializeField] private float rotationStepDistance = 0.08f;

        private ScaleOutGesture scaleOut;
        private OneHandRotationGesture rotation;

        public override void Enter()
        {
            scaleOut = new ScaleOutGesture(
                gesture.ScaleOutMinDistance, gesture.ScaleOutMaxRatio);
            rotation = new OneHandRotationGesture(rotationStepDistance);
            gesture.StateChanged += OnGestureChanged;
        }

        public override void Exit()
        {
            gesture.StateChanged -= OnGestureChanged;
            scaleOut.Reset();
            rotation.Reset();
        }

        private void OnGestureChanged(TwoHandGestureState state)
        {
            if (scaleOut.Update(state))
            {
                ExitToSelection();
                return;
            }

            if (!rotationEnabled)
                return;

            int steps = rotation.Update(state, head.right);
            if (steps != 0)
                puzzleController.Rotate(-steps * rotationStepDegrees);
        }

        private void ExitToSelection()
        {
            puzzleController.Exit();
            puzzleController.HideAll();
            parent.SetCurrentState(gameStartState.RouteId);
        }
    }
}
