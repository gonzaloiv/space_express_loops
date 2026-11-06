using System.Threading.Tasks;
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
        [SerializeField] private float saveTimeoutSecs = 5f;

        [Inject] private MemoryDataClient memoryDataClient;
        [Inject] private UnityPlayerDataClient playerDataClient;

        private Task pendingSave = Task.CompletedTask;

        public override void Enter()
        {
            PuzzleData puzzle = memoryDataClient.Get<PuzzleData>();
            PuzzleRunResult result = memoryDataClient.Get<PuzzleRunResult>();
            pendingSave = result.Solved ? SaveSolve(puzzle, result.Grade) : Task.CompletedTask;

            string cue = result.Solved ? puzzle.CompleteKey : puzzle.FailKey;
            ttsPresenter.Play(new[] { cue }, ReturnWhenSaved);
        }

        public override void Exit()
        {
            ttsPresenter.Stop();
        }

        private async void ReturnWhenSaved()
        {
            await Task.WhenAny(pendingSave, Task.Delay(System.TimeSpan.FromSeconds(saveTimeoutSecs)));
            ReturnToSelection();
        }

        private async Task SaveSolve(PuzzleData puzzle, int grade)
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            if (!RunProgressCookies.RecordSolve(playerData, puzzle.id, grade))
                return;

            try
            {
                await playerDataClient.Put(playerData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save the solve for {puzzle.id}: {e}");
            }
        }

        private void ReturnToSelection()
        {
            puzzleController.Exit();
            puzzleController.HideAll();
            parent.SetCurrentState(nextState.RouteId);
        }
    }
}
