using System;
using System.Threading.Tasks;
using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Persistence;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.TTS;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class RunEndingState : MonoState
    {
        [SerializeField] private MonoState nextState;
        [SerializeField] private LevelSelector levelSelector;
        [SerializeField] private RunEndingView endingView;
        [SerializeField] private TtsPresenter ttsPresenter;
        [SerializeField] private float saveTimeoutSecs = 5f;

        [Inject] private MemoryDataClient memoryDataClient;
        [Inject] private IDataProvider dataProvider;

        private Task pendingSave = Task.CompletedTask;
        private bool active;

        public override void Enter()
        {
            active = true;

            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            RunProgress progress = RunProgressCookies.Build(levelSelector, playerData);
            if (!progress.IsComplete || RunProgressCookies.HasSeenEnding(playerData, progress.Count))
            {
                ReturnToMap();
                return;
            }

            RunProgressCookies.MarkEndingSeen(playerData, progress.Count);
            pendingSave = SaveEnding(playerData);
            PlayEnding(progress);
        }

        public override void Exit()
        {
            active = false;
            endingView.Stop();
            ttsPresenter.Stop();
        }

        private async void PlayEnding(RunProgress progress)
        {
            await Task.WhenAny(pendingSave, Task.Delay(TimeSpan.FromSeconds(saveTimeoutSecs)));
            if (!active)
                return;

            PuzzleData[] levels = RunMap.ReadLevels(levelSelector);
            PuzzleMap map = RunMap.Build(levels, progress);
            endingView.Play(map, ReturnToMap);
        }

        private async Task SaveEnding(PlayerData playerData)
        {
            try
            {
                await dataProvider.Put(playerData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save the run ending: {e}");
            }
        }

        private void ReturnToMap() => parent.SetCurrentState(nextState.RouteId);
    }
}
