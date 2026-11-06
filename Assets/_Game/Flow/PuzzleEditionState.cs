using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Input;
using DigitalLove.TTS;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class PuzzleEditionState : MonoState
    {
        [SerializeField] private MonoState gameStartState;
        [SerializeField] private MonoState completeState;
        [SerializeField] private TwoHandGestureInput gesture;
        [SerializeField] private SpaceShapingController puzzleController;
        [SerializeField] private TtsPresenter ttsPresenter;
        [SerializeField] private Transform head;
        [SerializeField] private bool rotationEnabled;
        [SerializeField] private float rotationStepDegrees = 15f;
        [SerializeField] private float rotationStepDistance = 0.08f;

        [Inject] private MemoryDataClient memoryDataClient;

        private ScaleOutGesture scaleOut;
        private OneHandRotationGesture rotation;

        public override void Enter()
        {
            scaleOut = new ScaleOutGesture(
                gesture.ScaleOutMinDistance, gesture.ScaleOutMaxRatio);
            rotation = new OneHandRotationGesture(rotationStepDistance);
            gesture.StateChanged += OnGestureChanged;
            puzzleController.Solved += OnSolved;
            puzzleController.Failed += OnFailed;
        }

        public override void Exit()
        {
            gesture.StateChanged -= OnGestureChanged;
            puzzleController.Solved -= OnSolved;
            puzzleController.Failed -= OnFailed;
            scaleOut.Reset();
            rotation.Reset();
        }

        private void OnSolved() => EndRun(true);

        private void OnFailed() => EndRun(false);

        private void EndRun(bool solved)
        {
            memoryDataClient.Put(new PuzzleRunResult(solved, puzzleController.Grade));
            parent.SetCurrentState(completeState.RouteId);
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
            ttsPresenter.Stop();
            puzzleController.Exit();
            puzzleController.HideAll();
            parent.SetCurrentState(gameStartState.RouteId);
        }
    }
}
