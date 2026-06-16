using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Game.Persistence;
using Reflex.Attributes;
using UnityEngine;
using DigitalLove.Game.TTS;
using DigitalLove.Game.UI;
using DigitalLove.Casual.UI;

namespace DigitalLove.Game.Flow
{
    public class GameStartState : MonoState
    {
        [SerializeField] private LevelContainer levelContainer;
        [SerializeField] private RoundSelector roundSelector;
        [SerializeField] private GameSnapshotClient gameSnapshotClient;
        [SerializeField] private MonoState nextState;

        [Header("UI")]
        [SerializeField] private TTSHelper ttsHelper;
        [SerializeField] private HighScorePosterBehaviour highScorePoster;
        [SerializeField] private ReviewPanel reviewPanel;

        [Header("Debug")]
        [SerializeField] private PlayerData playerData;
        [SerializeField] private GameSnapshot gameSnapshot;

        [Inject] private MemoryDataClient memoryDataClient;
        [Inject] private UnityPlayerDataClient unityPlayerDataClient;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            levelContainer.Init();
            highScorePoster.Hide();
            reviewPanel.Hide();
        }

        public override void Enter()
        {
            InitData();
            highScorePoster.Show();
            reviewPanel.Show();

            if (gameSnapshot.HasPlanets)
                ResumeLevel();
            else
                BeginFreshLevel();
        }

        private void InitData()
        {
            playerData = memoryDataClient.Get<PlayerData>();
            gameSnapshot = playerData.HasCookie(GameSnapshot.CookieKey)
                ? GameSnapshot.FromCookieMetadata(playerData.GetCookieById(GameSnapshot.CookieKey).metadata)
                : new();
            gameSnapshot.SetOnUpdated(() => gameSnapshotClient.SetHasToUpdate());
            memoryDataClient.Put(gameSnapshot);
            roundSelector.SetCurrentRound(gameSnapshot.roundIndex);
            levelContainer.SyncIdCounters(gameSnapshot);
        }

        private void BeginFreshLevel()
        {
            PersistPlayerDataThenStartFresh();
        }

        private async void PersistPlayerDataThenStartFresh()
        {
            if (unityPlayerDataClient.IsReady)
                await unityPlayerDataClient.Put(playerData);

            levelContainer.StartFresh(() =>
            {
                levelContainer.SpawnRound(roundSelector.CurrentRound, gameSnapshot);
                gameSnapshot.RecalculateLettersRequiredForRound(roundSelector.CurrentRound.lettersIncreaseMultiplier);
                FinishStart("welcome_message");
            });
        }

        private void ResumeLevel()
        {
            gameSnapshot.EnsureLettersRequiredForRound(roundSelector.CurrentRound.lettersIncreaseMultiplier);
            levelContainer.RestoreFromSnapshot(gameSnapshot, () => FinishStart("welcome_back_message"));
        }

        private void FinishStart(string messageKey)
        {
            ttsHelper.SetInFrontOfCameraOrDefault(true);
            ttsHelper.Say(messageKey, ToNextState);
        }

        private void ToNextState()
        {
            if (nextState != null)
                parent.SetCurrentState(nextState.RouteId);
        }

        public override void Exit() { }
    }
}
