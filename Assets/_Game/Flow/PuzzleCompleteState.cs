using System;
using System.Collections;
using System.Threading.Tasks;
using DigitalLove.Audio;
using DigitalLove.Casual.Levels;
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
        [SerializeField] private MonoState retryState;
        [SerializeField] private LevelSelector levelSelector;
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
        [Inject] private IDataProvider dataProvider;

        private Task pendingSave = Task.CompletedTask;
        private Coroutine sequence;
        private int signpostIndex = -1;
        private bool showExitPrompt;
        private bool active;

        public override void Enter()
        {
            active = true;
            PuzzleData puzzle = memoryDataClient.Get<PuzzleData>();
            PuzzleRunResult result = memoryDataClient.Get<PuzzleRunResult>();
            JobOpenResult open = JobOpenResult.None;
            if (result.Solved)
                pendingSave = SaveSolve(puzzle, result.Grade);
            else
                open = OpenNext(puzzle);

            signpostIndex = open.SignpostIndex;
            showExitPrompt = open.ShowExitPrompt;
            cuePlayer.Play(result.Solved ? arriveCue : failCue);
            puzzleController.StarStamped += OnStarStamped;
            sequence = result.Solved
                ? StartCoroutine(PlaySolve(puzzle, result))
                : StartCoroutine(PlayFail(puzzle));
        }

        public override void Exit()
        {
            active = false;
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
            if (solved)
                ttsPresenter.Play(new[] { cue }, ReturnWhenSaved);
            else
                ttsPresenter.Play(new[] { cue }, Restart);
        }

        private JobOpenResult OpenNext(PuzzleData puzzle)
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            JobOpenResult open = RunProgressCookies.OpenNext(playerData, levelSelector, puzzle);
            pendingSave = open.Changed
                ? Put(playerData, "opened job")
                : Task.CompletedTask;
            return open;
        }

        private async void Restart()
        {
            await Task.WhenAny(pendingSave, Task.Delay(TimeSpan.FromSeconds(saveTimeoutSecs)));
            if (!active)
                return;

            memoryDataClient.Put(new PuzzleRestart(true, signpostIndex, showExitPrompt));
            puzzleController.Exit();
            parent.SetCurrentState(retryState.RouteId);
        }

        private async void ReturnWhenSaved()
        {
            await Task.WhenAny(pendingSave, Task.Delay(TimeSpan.FromSeconds(saveTimeoutSecs)));
            if (!active)
                return;

            ReturnToSelection();
        }

        private async Task SaveSolve(PuzzleData puzzle, int grade)
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            if (RunProgressCookies.RecordSolve(playerData, puzzle.id, grade))
                await Put(playerData, $"solve for {puzzle.id}");
        }

        private async Task Put(PlayerData playerData, string what)
        {
            try
            {
                await dataProvider.Put(playerData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save the {what}: {e}");
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
