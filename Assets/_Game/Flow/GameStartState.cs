using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Game.Persistence;
using Newtonsoft.Json;
using Reflex.Attributes;
using UnityEngine;
using DigitalLove.Game.TTS;
using DigitalLove.Global;
using DigitalLove.Casual.Flow;

namespace DigitalLove.Game.Flow
{
    public class GameStartState : MonoState
    {
        [SerializeField] private LevelContainer levelContainer;
        [SerializeField] private RoundSelector roundSelector;
        [SerializeField] private GameSnapshotClient gameSnapshotClient;
        [SerializeField] private MonoState nextState;
        [SerializeField] private TTSHelper ttsHelper;

        [Header("Debug")]
        [SerializeField] private DebugBool addInitialScore;
        [SerializeField] private int initialLetters;
        [SerializeField] private int initialMoney;

        [SerializeField] private PlayerData playerData;
        private GameSnapshot gameSnapshot;

        [Inject] private MemoryDataClient memoryDataClient;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            levelContainer.HideAll();
        }

        public override void Enter()
        {
            InitData();
            if (addInitialScore.Value)
                gameSnapshot.IncreaseLettersAndMoney(initialLetters, initialMoney);
            if (!gameSnapshot.HasPlanets)
            {
                SpawnLevelFromInitialRound();
            }
            else
            {
                RespawnFromData();
            }
        }

        private void InitData()
        {
            playerData = memoryDataClient.Get<PlayerData>();
            gameSnapshot = playerData.HasCookie(GameSnapshot.CookieKey) ? playerData.GetGameSnapshot() : new();
            gameSnapshot.SetOnUpdated(() => gameSnapshotClient.SetHasToUpdate());
            memoryDataClient.Put(gameSnapshot);
            roundSelector.SetCurrentRound(gameSnapshot.roundIndex);
            levelContainer.SyncIdCounters(gameSnapshot);
        }

        private void SpawnLevelFromInitialRound()
        {
            levelContainer.SetRoomBasedPose(() =>
            {
                levelContainer.SpawnInitialRound(roundSelector.CurrentRound, gameSnapshot);
                gameSnapshot.RecalculateLettersRequiredForRound(roundSelector.CurrentRound.lettersIncreaseMultiplier);
                SayIntro();
            });
        }

        private void SayIntro()
        {
            void SayRoundIntro() => ttsHelper.SayRoundIntro(roundSelector.CurrentRound, ToNextState);
            bool isFirstTry = memoryDataClient.Get<Play>().IsFirstTry;
            ttsHelper.SetInFrontOfCameraOrDefault(true);
            if (isFirstTry)
            {
                ttsHelper.Say("welcome_message", SayRoundIntro);
            }
            else
            {
                SayRoundIntro();
            }
        }

        private void RespawnFromData()
        {
            levelContainer.SetRoomBasedPose(() =>
            {
                roundSelector.SetCurrentRound(gameSnapshot.roundIndex);
                levelContainer.RespawnFromData(gameSnapshot);
                ttsHelper.SetInFrontOfCameraOrDefault(true);
                ttsHelper.Say("welcome_back_message", ToNextState);
            });
        }

        private void ToNextState()
        {
            if (nextState != null)
                parent.SetCurrentState(nextState.RouteId);
        }

        public override void Exit() { }
    }

    public static class GameStartStateExtensions
    {
        public static GameSnapshot GetGameSnapshot(this PlayerData playerData)
        {
            string metadata = playerData.GetCookieById(GameSnapshot.CookieKey).metadata;
            return JsonConvert.DeserializeObject<GameSnapshot>(metadata);
        }
    }
}