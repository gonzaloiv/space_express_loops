using System;
using System.Collections.Generic;
using System.Linq;
using DigitalLove.Game.Nodes;
using DigitalLove.Game.Persistence;
using DigitalLove.Game.Spaceships;
using DigitalLove.XR.MRUtilityKit;
using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class LevelContainer : MonoBehaviour
    {
        [SerializeField] private MRUKRoomAnchorsContainer mrukRoomAnchorsContainer;
        [SerializeField] private MrukRoomLocalPlacement roomPlacement;
        [SerializeField] private PlanetsSpawner planetsSpawner;
        [SerializeField] private SpaceshipsSpawner spaceshipsSpawner;
        [SerializeField] private HubsSpawner hubsSpawner;

        public PlanetsSpawner PlanetsSpawner => planetsSpawner;
        public SpaceshipsSpawner SpaceshipsSpawner => spaceshipsSpawner;
        public HubsSpawner HubsSpawner => hubsSpawner;

        public void Init()
        {
            spaceshipsSpawner.SetPlanetAvailability(new LevelLoopPlanetAvailability(planetsSpawner, hubsSpawner));
            HideAll();
        }

        public void SyncIdCounters(GameSnapshot gameSnapshot)
        {
            planetsSpawner.SyncIdsFromSnapshot(gameSnapshot.planets.Select(p => p.id));
            spaceshipsSpawner.SyncIdsFromSnapshot(gameSnapshot.loops.Select(l => l.spaceshipId));
            hubsSpawner.SyncIdsFromSnapshot(gameSnapshot.hubs.Select(h => h.id));
        }

        public void HideAll()
        {
            planetsSpawner.ResetPool();
            spaceshipsSpawner.ResetPool();
            hubsSpawner.ResetPool();
            roomPlacement.Clear();
        }

        public void ResetForRestart()
        {
            HideAll();
            mrukRoomAnchorsContainer.ClearRoomAnchors();
        }

        public void SpawnRound(RoundData roundData, GameSnapshot gameSnapshot)
        {
            SyncIdCounters(gameSnapshot);

            if (roundData.shouldSpawnSpaceship)
                SpawnSpaceship(gameSnapshot);

            SpawnPlanets(gameSnapshot, roundData);
            FinalizeSpawn(gameSnapshot);
        }

        public void RestoreFromSnapshot(GameSnapshot gameSnapshot, Action onComplete)
        {
            SetRoomBasedPose(() =>
            {
                RespawnFromData(gameSnapshot);
                onComplete?.Invoke();
            });
        }

        private void SpawnSpaceship(GameSnapshot gameSnapshot)
        {
            HubBehaviour hub = hubsSpawner.SpawnNew();
            gameSnapshot.AddHub(hubsSpawner.CreateHubData(hub));
            SpaceshipBehaviour spaceship = spaceshipsSpawner.SpawnNew(hub);
            gameSnapshot.SaveLoop(new LoopData
            {
                spaceshipId = spaceship.Id,
                colorCode = spaceship.ColorCode,
                hubId = spaceship.HubId
            });
        }

        private void SpawnPlanets(GameSnapshot gameSnapshot, RoundData roundData)
        {
            List<PlanetData> roundPlanets = planetsSpawner.GeneratePlanetDataFromPlanetsSeed(
                roundData.planetsToAdd.GetRandomValue(),
                roundData.planetSeed,
                roundData.distanceBetweenPlanetsMultiplier
            );
            gameSnapshot.AddPlanets(roundPlanets);
            planetsSpawner.SpawnPlanets(gameSnapshot.planets);
        }

        private void RespawnFromData(GameSnapshot gameSnapshot)
        {
            roomPlacement.SyncFromSnapshot(
                gameSnapshot.hubs,
                gameSnapshot.planets,
                hubsSpawner.HubPlacementRadius);
            hubsSpawner.SpawnHubs(gameSnapshot.hubs);
            planetsSpawner.SpawnPlanets(gameSnapshot.planets);

            foreach (LoopData loop in gameSnapshot.loops)
                SpawnSpaceshipFromLoop(loop, gameSnapshot);

            FinalizeSpawn(gameSnapshot);
        }

        private void FinalizeSpawn(GameSnapshot gameSnapshot)
        {
            PlanetRouteColorSync.Apply(gameSnapshot, planetsSpawner, hubsSpawner, spaceshipsSpawner);
            spaceshipsSpawner.RefreshGrabbablesAtStation();
        }

        private void SpawnSpaceshipFromLoop(LoopData loop, GameSnapshot gameSnapshot)
        {
            HubBehaviour hub = hubsSpawner.GetById(loop.hubId)
                ?? hubsSpawner.GetOrSpawn(loop.hubId, gameSnapshot.GetHubById(loop.hubId));
            List<PlanetBehaviour> destinations = loop.ResolveDestinations(planetsSpawner.GetById);
            spaceshipsSpawner.SpawnRestored(loop.spaceshipId, hub, destinations, loop.colorCode);
        }

        public void StartFresh(Action onComplete)
        {
            mrukRoomAnchorsContainer.Clear();
            SetRoomBasedPose(onComplete);
        }

        public void SetRoomBasedPose(Action onComplete)
        {
            MRUKRoom room = MRUK.Instance.GetCurrentRoom();
            if (room == null)
            {
                Debug.LogWarning("[LevelContainer] No MRUK room available; skipping room-based pose setup.");
                onComplete();
                return;
            }

            Pose originPose = new()
            {
                position = room.Center(),
                rotation = room.transform.rotation
            };
            mrukRoomAnchorsContainer.InitAndLoadRoomAnchors("UniqueRoomName", originPose, anchors =>
            {
                if (!mrukRoomAnchorsContainer.TryGetOVRAnchorPose(out Pose toSet))
                {
                    Debug.LogWarning("[LevelContainer] Failed to load room anchor; falling back to room center.");
                    toSet = originPose;
                }

                transform.position = toSet.position;
                transform.rotation = toSet.rotation;
                onComplete();
            });
        }

        public void ResetLetters()
        {
            planetsSpawner.All.ForEach(planet => planet.PlanetStore.ResetLetters());
        }
    }
}
