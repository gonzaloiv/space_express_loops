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
            gameSnapshot.hubs ??= new();
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
            mrukRoomAnchorsContainer.ClearEntityAnchors();
        }

        public void SpawnInitialRound(RoundData roundData, GameSnapshot gameSnapshot)
        {
            SpawnRound(roundData, gameSnapshot);
        }

        public void SpawnRound(RoundData roundData, GameSnapshot gameSnapshot)
        {
            if (roundData.shouldSpawnSpaceship)
                SpawnSpaceship(gameSnapshot);

            SpawnPlanets(gameSnapshot, roundData);
            PlanetRouteColorSync.Apply(gameSnapshot, planetsSpawner, hubsSpawner, spaceshipsSpawner);
            spaceshipsSpawner.RefreshGrabbablesAtStation();
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

        public void RespawnFromData(GameSnapshot gameSnapshot)
        {
            gameSnapshot.hubs ??= new();
            roomPlacement.SyncFromSnapshot(
                gameSnapshot.hubs,
                gameSnapshot.planets,
                hubsSpawner.HubPlacementRadius);
            hubsSpawner.SpawnHubs(gameSnapshot.hubs);
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

        private HubBehaviour ResolveHubForLoop(LoopData loop, GameSnapshot gameSnapshot) =>
            hubsSpawner.GetById(loop.hubId) ?? hubsSpawner.GetOrSpawn(loop.hubId, gameSnapshot.GetHubById(loop.hubId));

        public void SetRoomBasedPose(Action onComplete)
        {
            MRUKRoom room = MRUK.Instance.GetCurrentRoom();
            Pose originPose = new()
            {
                position = room.Center(),
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

        public void ResetLetters()
        {
            planetsSpawner.All.ForEach(planet => planet.PlanetStore.ResetLetters());
        }
    }
}
