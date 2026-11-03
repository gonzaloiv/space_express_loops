using System.Collections;
using DigitalLove.Casual.Flow;
using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Game.Persistence;
using DigitalLove.Global;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class GameEndState : MonoState
    {
        [SerializeField] private MonoState nextState;
        [SerializeField] private LevelContainer levelContainer;
        [SerializeField] private GameSnapshotClient gameSnapshotClient;
        [SerializeField] private StringValue highScoreCookieKey;
        [SerializeField] private GameEndPanel gameEndPanel;

        [Header("DEBUG")]
        [SerializeField] private DebugBool jumpCountdown;

        [Inject] private MemoryDataClient memoryDataClient;
        [Inject] private UnityPlayerDataClient unityPlayerDataClient;

        private Coroutine countdownRoutine;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            gameEndPanel.Hide();
        }

        public override void Enter()
        {
            SaveHighScore();
        }

        private void SaveHighScore()
        {
            GameSnapshot gameSnapshot = memoryDataClient.Get<GameSnapshot>();
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            if (playerData.TrySaveHighScoreMoney(highScoreCookieKey, gameSnapshot.CurrentLetters))
            {
                memoryDataClient.Put(playerData);
                PersistPlayerData(playerData);
                ShowGameEndPanel(gameSnapshot.CurrentLetters);
            }
            else
            {
                gameEndPanel.Show();
                StartCountdown();
            }
        }

        private void ShowGameEndPanel(int currentLetters)
        {
            if (!jumpCountdown.Value)
            {
                StopCountdown();
                countdownRoutine = StartCoroutine(RestartAfterFrame());
                return;
            }

            gameEndPanel.ShowWithNewHighScore(currentLetters);
            StartCountdown();
        }

        private IEnumerator RestartAfterFrame()
        {
            yield return null;
            countdownRoutine = null;
            Restart();
        }

        private void StartCountdown()
        {
            StopCountdown();
            countdownRoutine = StartCoroutine(RunCountdown());
        }

        private IEnumerator RunCountdown()
        {
            for (int seconds = gameEndPanel.SecsToBackToTrackSelection; seconds > 0; seconds--)
            {
                gameEndPanel.PlayCountdownTick();
                gameEndPanel.ShowCountdown(seconds);
                yield return new WaitForSeconds(1f);
            }

            countdownRoutine = null;
            Restart();
        }

        private void StopCountdown()
        {
            if (countdownRoutine == null)
                return;

            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }

        public override void Exit()
        {
            StopCountdown();
        }

        public void Restart()
        {
            StopCountdown();
            gameEndPanel.Hide();

            levelContainer.ResetForRestart();

            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            GameSnapshot gameSnapshot = new();
            gameSnapshot.SetOnUpdated(() => gameSnapshotClient.SetHasToUpdate());
            memoryDataClient.Get<Play>().IncreaseTries();

            GameSnapshotStore.Save(gameSnapshot, playerData, memoryDataClient);
            PersistPlayerData(playerData);

            parent.SetCurrentState(nextState.RouteId);
        }

        private async void PersistPlayerData(PlayerData playerData)
        {
            await GameSnapshotStore.SavePlayerDataAsync(playerData, unityPlayerDataClient);
        }
    }
}
