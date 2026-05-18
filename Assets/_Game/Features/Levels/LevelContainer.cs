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
        [SerializeField] private PlanetsSpawner planetsSpawner;
        [SerializeField] private SpaceshipsSpawner spaceshipsSpawner;
        [SerializeField] private PlanetBaseBehaviour basePlanet;

        public PlanetsSpawner PlanetsSpawner => planetsSpawner;
        public SpaceshipsSpawner SpaceshipsSpawner => spaceshipsSpawner;
        public PlanetBaseBehaviour BasePlanet => basePlanet;

        public void SyncIdCounters(GameSnapshot gameSnapshot)
        {
            planetsSpawner.SyncIdsFromSnapshot(gameSnapshot.planets.Select(p => p.id));
            spaceshipsSpawner.SyncIdsFromSnapshot(gameSnapshot.loops.Select(l => l.spaceshipId));
        }

        public void HideAll()
        {
            planetsSpawner.HideAll();
            spaceshipsSpawner.HideAll();
            basePlanet.SetActive(false);
        }

        public void SpawnInitialRound(RoundData roundData, GameSnapshot gameSnapshot)
        {
            basePlanet.SetActive(true);
            SpawnRound(roundData, gameSnapshot);
        }

        public void SpawnRound(RoundData roundData, GameSnapshot gameSnapshot)
        {
            List<PlanetData> roundPlanets = planetsSpawner.GeneratePlanetDataFromPlanetsSeed(roundData.planetsToAdd.GetRandomValue(), roundData.planetSeed, gameSnapshot.planets);
            gameSnapshot.AddPlanets(roundPlanets);
            planetsSpawner.SpawnPlanets(gameSnapshot.planets);
            if (roundData.shouldSpawnSpaceship)
                SpawnSpaceship(gameSnapshot);
        }

        private void SpawnSpaceship(GameSnapshot gameSnapshot)
        {
            SpaceshipBehaviour spaceship = spaceshipsSpawner.SpawnNew(basePlanet);
            gameSnapshot.AddLoop(new LoopData
            {
                spaceshipId = spaceship.Id,
                colorCode = spaceship.ColorCode
            });
        }

        public void RespawnFromData(GameSnapshot gameSnapshot)
        {
            basePlanet.SetActive(true);

            planetsSpawner.SpawnPlanets(gameSnapshot.planets);

            foreach (LoopData loop in gameSnapshot.loops)
            {
                if (loop.HasDestination)
                {
                    PlanetBehaviour destination = planetsSpawner.GetById(loop.destinationId);
                    spaceshipsSpawner.SpawnFromLoop(
                        loop.spaceshipId,
                        basePlanet,
                        destination,
                        loop.colorCode);
                    PlanetRouteColorSync.ApplyDestinationRouteColor(destination, loop.colorCode, spaceshipsSpawner);
                }
                else
                {
                    spaceshipsSpawner.SpawnIdle(loop.spaceshipId, basePlanet, loop.colorCode);
                }
            }
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