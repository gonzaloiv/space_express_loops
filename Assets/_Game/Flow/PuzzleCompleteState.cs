using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Persistence;
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
        [Inject] private UnityPlayerDataClient playerDataClient;

        public override void Enter()
        {
            PuzzleData puzzle = memoryDataClient.Get<PuzzleData>();
            PuzzleRunResult result = memoryDataClient.Get<PuzzleRunResult>();
            if (result.Solved)
                SaveSolve(puzzle, result.Grade);

            string cue = result.Solved ? puzzle.CompleteKey : puzzle.FailKey;
            ttsPresenter.Play(new[] { cue }, ReturnToSelection);
        }

        public override void Exit()
        {
            ttsPresenter.Stop();
        }

        private async void SaveSolve(PuzzleData puzzle, int grade)
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            if (RunProgressCookies.RecordSolve(playerData, puzzle.id, grade))
                await playerDataClient.Put(playerData);
        }

        private void ReturnToSelection()
        {
            puzzleController.Exit();
            puzzleController.HideAll();
            parent.SetCurrentState(nextState.RouteId);
        }
    }
}
