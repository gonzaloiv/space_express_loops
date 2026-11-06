using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.TTS;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class PuzzleCompleteState : MonoState
    {
        [SerializeField] private MonoState nextState;
        [SerializeField] private SpaceShapingController puzzleController;
        [SerializeField] private TtsPresenter ttsPresenter;

        [Inject] private MemoryDataClient memoryDataClient;

        public override void Enter()
        {
            PuzzleData puzzle = memoryDataClient.Get<PuzzleData>();
            ttsPresenter.Play(new[] { puzzle.CompleteKey }, ReturnToSelection);
        }

        public override void Exit()
        {
            ttsPresenter.Stop();
        }

        private void ReturnToSelection()
        {
            puzzleController.Exit();
            puzzleController.HideAll();
            parent.SetCurrentState(nextState.RouteId);
        }
    }
}
