using System;
using System.Collections.Generic;
using System.Linq;
using DigitalLove.Game.Planets;
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

        public void SyncIdCounters(GameSnapshot gameSnapshot)
        {
            planetsSpawner.SyncIdsFromSnapshot(gameSnapshot.planets.Select(p => p.id));
            spaceshipsSpawner.SyncIdsFromSnapshot(gameSnapshot.loops.Select(l => l.spaceshipId));
            gameSnapshot.hubs ??= new();
            hubsSpawner.SyncIdsFromSnapshot(gameSnapshot.hubs.Select(h => h.id));
        }

        public void HideAll()
        {
            spaceshipsSpawner.InitializePool();
            planetsSpawner.HideAll();
            spaceshipsSpawner.HideAll();
            hubsSpawner.HideAll();
            roomPlacement.Clear();
        }

        public void ResetForRestart()
        {
            HideAll();
            mrukRoomAnchorsContainer.ClearEntityAnchors();
        }

        public void SpawnInitialRound(RoundData roundData, GameSnapshot gameSnapshot)
        {
            SpawnRound(roundData, gameSnapshot);
        }

        public void SpawnRound(RoundData roundData, GameSnapshot gameSnapshot)
        {
            List<PlanetData> roundPlanets = planetsSpawner.GeneratePlanetDataFromPlanetsSeed(roundData.planetsToAdd.GetRandomValue(), roundData.planetSeed);
            gameSnapshot.AddPlanets(roundPlanets);
            planetsSpawner.SpawnPlanets(gameSnapshot.planets);
            PlanetRouteColorSync.SyncPlanetRouteColors(gameSnapshot, planetsSpawner, spaceshipsSpawner);
            if (roundData.shouldSpawnSpaceship)
                SpawnSpaceship(gameSnapshot);
            else
                PlanetRouteColorSync.SyncHubRouteColors(gameSnapshot, hubsSpawner, spaceshipsSpawner);
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
            PlanetRouteColorSync.SyncHubRouteColors(gameSnapshot, hubsSpawner, spaceshipsSpawner);
        }

        public void RespawnFromData(GameSnapshot gameSnapshot)
        {
            gameSnapshot.hubs ??= new();
            roomPlacement.SyncFromSnapshot(
                gameSnapshot.planets,
                gameSnapshot.hubs,
                hubsSpawner.All[0].PlanetBody.Radius);
            planetsSpawner.SpawnPlanets(gameSnapshot.planets);
            PlanetRouteColorSync.SyncPlanetRouteColors(gameSnapshot, planetsSpawner, spaceshipsSpawner);

            foreach (LoopData loop in gameSnapshot.loops)
            {
                HubBehaviour hub = ResolveHubForLoop(loop, gameSnapshot);

                if (loop.HasDestinations)
                {
                    List<PlanetBehaviour> destinations = loop.destinationIds
                        .ConvertAll(id => planetsSpawner.GetById(id));
                    spaceshipsSpawner.SpawnFromLoop(
                        loop.spaceshipId,
                        hub,
                        destinations,
                        loop.colorCode);
                }
                else
                {
                    spaceshipsSpawner.SpawnIdle(loop.spaceshipId, hub, loop.colorCode);
                }
            }

            PlanetRouteColorSync.SyncHubRouteColors(gameSnapshot, hubsSpawner, spaceshipsSpawner);
        }

        private HubBehaviour ResolveHubForLoop(LoopData loop, GameSnapshot gameSnapshot)
        {
            HubData hubData = gameSnapshot.GetHubById(loop.hubId);
            HubBehaviour hub = hubsSpawner.GetById(loop.hubId);
            if (hub != null && hub.IsActive)
                return hub;
            return hubsSpawner.SpawnWithId(loop.hubId, hubData);
        }

        public void SetRoomBasedPose(Action onComplete)
        {
            MRUKRoom room = MRUK.Instance.GetCurrentRoom();
            Pose originPose = new()
            {
                position = room.GetRoomBounds().center,
                rotation = room.transform.rotation
            };
            mrukRoomAnchorsContainer.InitAndLoadRoomAnchors("UniqueRoomName", originPose, anchors =>
            {
                Pose toSet = mrukRoomAnchorsContainer.OVRAnchorPose;
                transform.position = toSet.position;
                transform.rotation = toSet.rotation;
                onComplete();
            });
        }
    }
}
