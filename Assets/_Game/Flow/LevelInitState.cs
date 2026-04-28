using System.Collections.Generic;
using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Game.Persistence;
using DigitalLove.Game.Planets;
using Newtonsoft.Json;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class LevelInitState : MonoState
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
            bool spawnedFromData = false;
            bool hasPreviousData = InitData();
            if (hasPreviousData)
                spawnedFromData = TryToSpawnFromData();
            if (!spawnedFromData)
                SpawnNewLevel();
            if (nextState != null)
                parent.SetCurrentState(nextState.RouteId);
        }

        private bool InitData()
        {
            bool result = false;
            playerData = memoryDataClient.Get<PlayerData>();
            if (playerData.HasCookie(GameSnapshot.CookieKey))
            {
                string metadata = playerData.GetCookieById(GameSnapshot.CookieKey).metadata;
                gameSnapshot = JsonConvert.DeserializeObject<GameSnapshot>(metadata);
                result = true;
            }
            else
            {
                gameSnapshot = new();
            }
            gameSnapshot.SetOnUpdated(() => gameSnapshotClient.SetHasToUpdate());
            memoryDataClient.Put(gameSnapshot);
            return result;
        }

        private bool TryToSpawnFromData()
        {
            if (gameSnapshot.HasPlanets)
            {
                levelContainer.Respawn(gameSnapshot);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SpawnNewLevel()
        {
            RoundData roundData = roundSelector.GetCurrent();
            List<PlanetData> planetsData = levelContainer.SpawnNew(roundData);
            gameSnapshot.SetPlanets(planetsData);
        }

        public override void Exit()
        {

        }
    }
}