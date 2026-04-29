using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Game.Persistence;
using Newtonsoft.Json;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class GameStartState : MonoState
    {
        [SerializeField] private LevelContainer levelContainer;
        [SerializeField] private RoundSelector roundSelector;
        [SerializeField] private GameSnapshotClient gameSnapshotClient;
        [SerializeField] private MonoState nextState;

        [Header("Debug")]
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
            if (playerData.HasCookie(GameSnapshot.CookieKey))
            {
                string metadata = playerData.GetCookieById(GameSnapshot.CookieKey).metadata;
                gameSnapshot = JsonConvert.DeserializeObject<GameSnapshot>(metadata);
            }
            else
            {
                gameSnapshot = new();
            }
            gameSnapshot.SetOnUpdated(() => gameSnapshotClient.SetHasToUpdate());
            memoryDataClient.Put(gameSnapshot);
            roundSelector.SetCurrentRound(gameSnapshot.roundIndex);
        }

        private void SpawnLevelFromInitialRound()
        {
            levelContainer.SetRoomBasedPose(() =>
            {
                levelContainer.SpawnInitialRound(roundSelector.CurrentRound, gameSnapshot);
                ToNextState();
            });
        }

        private void RespawnFromData()
        {
            levelContainer.SetRoomBasedPose(() =>
            {
                roundSelector.SetCurrentRound(gameSnapshot.roundIndex);
                levelContainer.RespawnFromData(roundSelector.CurrentRound, gameSnapshot);
                ToNextState();
            });
        }

        private void ToNextState()
        {
            if (nextState != null)
                parent.SetCurrentState(nextState.RouteId);
        }

        public override void Exit()
        {

        }
    }
}