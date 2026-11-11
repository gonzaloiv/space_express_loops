using DigitalLove.FlowControl;
using UnityEngine;
using Reflex.Attributes;
using DigitalLove.Audio;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.DataAccess;
using DigitalLove.TTS;

namespace DigitalLove.Game.Flow
{
    public class PuzzleStartState : MonoState
    {
        [SerializeField] private MonoState nextState;
        [SerializeField] private SpaceShapingController puzzleController;
        [SerializeField] private TtsPresenter ttsPresenter;
        [SerializeField] private SynthCuePlayer systemPlayer;
        [SerializeField] private SynthPatch enterCue;

        [Inject] private MemoryDataClient memoryDataClient;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            puzzleController.HideAll();
            ttsPresenter.Stop();
        }

        public override void Enter()
        {
            systemPlayer.Play(enterCue);
            PuzzleRestart restart = memoryDataClient.Get<PuzzleRestart>();
            PuzzleData puzzle = memoryDataClient.Get<PuzzleData>();
            puzzleController.Enter(puzzle);
            if (restart.Value)
            {
                puzzleController.PlayEntropyReset(restart.SignpostIndex);
            }
            else
            {
                puzzleController.Glitch();
                if (!puzzle.noIntro)
                    ttsPresenter.Play(new[] { puzzle.IntroLinesKey });
            }

            parent.SetCurrentState(nextState.RouteId);
        }

        public override void Exit()
        {
        }
    }
}