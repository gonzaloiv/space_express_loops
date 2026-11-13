using DigitalLove.DataAccess;
using DigitalLove.Audio;
using DigitalLove.FlowControl;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Game.Spatial;
using DigitalLove.Player;
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
        [SerializeField] private HomeMenuInvokerBase menuInvoker;
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

        [Inject] private MemoryDataClient memoryDataClient;

        private PuzzleData puzzle;
        private float nextRefusalCueAt;
        private bool exiting;

        public override void Enter()
        {
            exiting = false;
            SubscribeMenu(true);
            puzzleController.Solved += OnSolved;
            puzzleController.Failed += OnFailed;
            puzzleController.FlightCrashed += OnFlightCrashed;
            puzzleController.Refused += OnRefused;
            puzzleController.PinchCommitted += OnPinchCommitted;
            PuzzleRestart restart = memoryDataClient.Get<PuzzleRestart>();
            memoryDataClient.Remove<PuzzleRestart>();
            puzzle = memoryDataClient.Get<PuzzleData>();
            ShowPrompts(puzzle, restart.Value);
            ShowContract(puzzle);
            if (restart.ShowExitPrompt)
                prompts.ShowAlways(GestureKind.Exit);
        }

        public override void Exit()
        {
            SubscribeMenu(false);
            puzzleController.Solved -= OnSolved;
            puzzleController.Failed -= OnFailed;
            puzzleController.FlightCrashed -= OnFlightCrashed;
            puzzleController.Refused -= OnRefused;
            puzzleController.PinchCommitted -= OnPinchCommitted;
            prompts.HideAll();
            contractCard.Hide();
            puzzle = null;
        }

        private void SubscribeMenu(bool subscribe)
        {
            if (subscribe)
                menuInvoker.homeMenuRequested += OnMenuRequested;
            else
                menuInvoker.homeMenuRequested -= OnMenuRequested;
        }

        private void OnMenuRequested()
        {
            if (exiting || Time.timeScale <= 0f)
                return;

            exiting = true;
            ExitToSelection();
        }

        private void ShowPrompts(PuzzleData puzzle, bool restart)
        {
            prompts.Show(GestureKind.Scale);
            if (puzzle.HasGoalLine)
                prompts.ShowAlways(GestureKind.Goal);
            if (puzzle.translatable)
                prompts.Show(GestureKind.Move);
            if (!puzzle.noIntro && !restart)
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
            if (!puzzle.noFail && prompts.SpendShown)
                promptSource.ReportSpend();
            memoryDataClient.Put(new PuzzleRunResult(solved, puzzleController.Grade));
            parent.SetCurrentState(completeState.RouteId);
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
