using DigitalLove.DataAccess;
using DigitalLove.Audio;
using DigitalLove.FlowControl;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Game.SpaceShaping.Cube;
using DigitalLove.Input;
using DigitalLove.TTS;
using DigitalLove.Game.GesturePrompts;
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
        [SerializeField] private GesturePromptPresenter prompts;
        [SerializeField] private GameGesturePromptSource promptSource;
        [SerializeField] private JobCardPanel contractCard;
        [SerializeField] private TtsPresenter ttsPresenter;
        [SerializeField] private SynthCuePlayer cuePlayer;
        [SerializeField] private SynthCuePlayer systemPlayer;
        [SerializeField] private SynthPatch crashCue;
        [SerializeField] private SynthPatch refusalCue;
        [SerializeField] private SynthPatch exitCue;
        [SerializeField] private float refusalCueCooldownSecs = 0.7f;
        [SerializeField] private Transform head;
        [SerializeField] private bool rotationEnabled;
        [SerializeField] private float rotationStepDegrees = 15f;
        [SerializeField] private float rotationStepDistance = 0.08f;

        [Inject] private MemoryDataClient memoryDataClient;

        private ScaleOutGesture scaleOut;
        private OneHandRotationGesture rotation;
        private PuzzleData puzzle;
        private float nextRefusalCueAt;

        public override void Enter()
        {
            scaleOut = new ScaleOutGesture(
                gesture.ScaleOutMinDistance, gesture.ScaleOutMaxRatio);
            rotation = new OneHandRotationGesture(rotationStepDistance);
            gesture.StateChanged += OnGestureChanged;
            puzzleController.Solved += OnSolved;
            puzzleController.Failed += OnFailed;
            puzzleController.FlightCrashed += OnFlightCrashed;
            puzzleController.Refused += OnRefused;
            puzzleController.PinchCommitted += OnPinchCommitted;
            puzzle = memoryDataClient.Get<PuzzleData>();
            ShowPrompts(puzzle);
            ShowContract(puzzle);
        }

        public override void Exit()
        {
            gesture.StateChanged -= OnGestureChanged;
            puzzleController.Solved -= OnSolved;
            puzzleController.Failed -= OnFailed;
            puzzleController.FlightCrashed -= OnFlightCrashed;
            puzzleController.Refused -= OnRefused;
            puzzleController.PinchCommitted -= OnPinchCommitted;
            scaleOut?.Reset();
            rotation?.Reset();
            prompts.HideAll();
            contractCard.Hide();
            puzzle = null;
        }

        private void ShowPrompts(PuzzleData puzzle)
        {
            prompts.Show(GestureKind.Scale);
            if (puzzle.translatable)
                prompts.Show(GestureKind.Move);
            if (!puzzle.noIntro)
                prompts.Show(GestureKind.Exit);
            if (!puzzle.noGrade)
                prompts.Show(GestureKind.Stars);
        }

        private void ShowContract(PuzzleData puzzle)
        {
            if (!puzzle.noIntro)
                contractCard.ShowInstant(puzzle);
        }

        private void OnSolved()
        {
            if (!puzzle.noGrade)
                promptSource.ReportStars();
            EndRun(true);
        }

        private void OnFailed() => EndRun(false);

        private void OnPinchCommitted()
        {
            if (!puzzle.noFail)
                prompts.RequestSpend();
        }

        private void OnFlightCrashed() => cuePlayer.Play(crashCue);

        private void OnRefused(ZoneRefusal reason)
        {
            if (reason != ZoneRefusal.OutOfEntropy || Time.unscaledTime < nextRefusalCueAt)
                return;

            nextRefusalCueAt = Time.unscaledTime + refusalCueCooldownSecs;
            cuePlayer.Play(refusalCue);
        }

        private void EndRun(bool solved)
        {
            if (!puzzle.noFail)
                promptSource.ReportSpend();
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
            promptSource.ReportExit();
            ttsPresenter.Stop();
            puzzleController.Exit();
            puzzleController.HideAll();
            systemPlayer.Play(exitCue);
            parent.SetCurrentState(gameStartState.RouteId);
        }
    }
}
