using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Game.Persistence;
using DigitalLove.Global;
using Newtonsoft.Json;
using Reflex.Attributes;
using UnityEngine;
using DigitalLove.Casual.Flow;


namespace DigitalLove.Game.Flow
{
    public class GameEndState : MonoState
    {
        [SerializeField] private MonoState nextState;
        [SerializeField] private LevelContainer levelContainer;
        [SerializeField] private GameSnapshotClient gameSnapshotClient;
        [SerializeField] private StringValue highScoreCookieKey;
        [SerializeField] private GameEndPanel gameEndPanel;

        [Inject] private MemoryDataClient memoryDataClient;
        [Inject] private UnityPlayerDataClient unityPlayerDataClient;

        public override void Enter()
        {
            gameEndPanel.countdownComplete += Restart;

            SaveHighScore();
        }

        private void SaveHighScore()
        {
            GameSnapshot gameSnapshot = memoryDataClient.Get<GameSnapshot>();
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            int highScore = playerData.GetHighScoreMoney(highScoreCookieKey);
            if (highScore < gameSnapshot.CurrentLetters)
            {
                playerData.SetHighScoreMoney(highScoreCookieKey, gameSnapshot.CurrentLetters);
                memoryDataClient.Put(playerData);
                PersistPlayerData(playerData);
                gameEndPanel.ShowWithNewHighScore(gameSnapshot.CurrentLetters);
            }
            else
            {
                gameEndPanel.Show();
            }
        }

        public override void Exit()
        {
            gameEndPanel.countdownComplete -= Restart;
        }

        public void Restart()
        {
            levelContainer.ResetForRestart();

            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            GameSnapshot gameSnapshot = new();
            gameSnapshot.SetOnUpdated(() => gameSnapshotClient.SetHasToUpdate());
            Cookie cookie = playerData.GetOrCreateCookie(GameSnapshot.CookieKey);
            cookie.metadata = JsonConvert.SerializeObject(gameSnapshot);
            memoryDataClient.Get<Play>().IncreaseTries();

            memoryDataClient.Put(gameSnapshot);
            memoryDataClient.Put(playerData);
            PersistPlayerData(playerData);

            parent.SetCurrentState(nextState.RouteId);
        }

        private async void PersistPlayerData(PlayerData playerData)
        {
            await unityPlayerDataClient.Put(playerData);
        }
    }
}
