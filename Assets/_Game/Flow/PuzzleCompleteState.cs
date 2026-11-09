using System.Collections;
using System.Threading.Tasks;
using DigitalLove.Audio;
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
        [SerializeField] private SynthCuePlayer cuePlayer;
        [SerializeField] private SynthCuePlayer systemPlayer;
        [SerializeField] private SynthPatch arriveCue;
        [SerializeField] private SynthPatch failCue;
        [SerializeField] private SynthPatch starCue;
        [SerializeField] private SynthPatch exitCue;
        [SerializeField] private SynthPatch glitchCue;
        [SerializeField] private float saveTimeoutSecs = 5f;

        [Inject] private MemoryDataClient memoryDataClient;
        [Inject] private UnityPlayerDataClient playerDataClient;

        private Task pendingSave = Task.CompletedTask;
        private Coroutine sequence;

        public override void Enter()
        {
            PuzzleData puzzle = memoryDataClient.Get<PuzzleData>();
            PuzzleRunResult result = memoryDataClient.Get<PuzzleRunResult>();
            pendingSave = result.Solved ? SaveSolve(puzzle, result.Grade) : Task.CompletedTask;
            cuePlayer.Play(result.Solved ? arriveCue : failCue);
            puzzleController.StarStamped += OnStarStamped;
            sequence = result.Solved
                ? StartCoroutine(PlaySolve(puzzle, result))
                : StartCoroutine(PlayFail(puzzle));
        }

        public override void Exit()
        {
            puzzleController.StarStamped -= OnStarStamped;
            if (sequence != null)
            {
                StopCoroutine(sequence);
                sequence = null;
            }

            ttsPresenter.Stop();
        }

        private IEnumerator PlaySolve(PuzzleData puzzle, PuzzleRunResult result)
        {
            yield return new WaitForSeconds(puzzleController.PlayArrival());
            if (!puzzle.noGrade)
                yield return new WaitForSeconds(puzzleController.PlayStamp(result.Grade));
            Speak(puzzle, true);
        }

        private IEnumerator PlayFail(PuzzleData puzzle)
        {
            systemPlayer.PlayOver(glitchCue);
            yield return new WaitForSeconds(puzzleController.Glitch());
            Speak(puzzle, false);
        }

        private void OnStarStamped(int index) => cuePlayer.Play(starCue);

        private void Speak(PuzzleData puzzle, bool solved)
        {
            string cue = solved ? puzzle.CompleteKey : puzzle.FailKey;
            ttsPresenter.Play(new[] { cue }, ReturnWhenSaved);
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
            systemPlayer.Play(exitCue);
            parent.SetCurrentState(nextState.RouteId);
        }
    }
}
