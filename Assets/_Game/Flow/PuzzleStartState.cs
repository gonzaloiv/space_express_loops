using DigitalLove.FlowControl;
using UnityEngine;
using Reflex.Attributes;
using DigitalLove.Audio;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Game.UI.Console;
using DigitalLove.DataAccess;
using DigitalLove.TTS;

namespace DigitalLove.Game.Flow
{
    public class PuzzleStartState : MonoState
    {
        [SerializeField] private MonoState nextState;
        [SerializeField] private SpaceShapingController puzzleController;
        [SerializeField] private ConsoleScreen console;
        [SerializeField] private TtsPresenter ttsPresenter;
        [SerializeField] private SynthCuePlayer systemPlayer;
        [SerializeField] private SynthPatch enterCue;

        [Inject] private MemoryDataClient memoryDataClient;

        private bool active;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            puzzleController.HideAll();
            ttsPresenter.Stop();
        }

        public override void Enter()
        {
            active = true;
            PuzzleRestart restart = memoryDataClient.Get<PuzzleRestart>();
            if (restart.Value)
            {
                // A real restart: the old field goes, the console reports the reset, then the field
                // comes back fresh instead of respawning over the previous attempt.
                puzzleController.HideAll();
                console.PlayMessage(restart.ConsoleLines(), StartPuzzle);
                return;
            }

            PuzzleData puzzle = memoryDataClient.Get<PuzzleData>();
            if (!puzzle.noIntro)
                ttsPresenter.Play(new[] { puzzle.IntroLinesKey });
            StartPuzzle();
        }

        public override void Exit() => active = false;

        private void StartPuzzle()
        {
            if (!active)
                return;

            systemPlayer.Play(enterCue);
            puzzleController.Enter(memoryDataClient.Get<PuzzleData>());
            puzzleController.Glitch();
            parent.SetCurrentState(nextState.RouteId);
        }
    }
}
