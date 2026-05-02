using System;
using System.Collections.Generic;
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
            List<PlanetData> roundPlanets = planetsSpawner.GeneratePlanetDataFromPlanetsSeed(roundData.planetsSeed, gameSnapshot.planets);
            gameSnapshot.AddPlanets(roundPlanets);
            planetsSpawner.SpawnPlanets(gameSnapshot.planets);
            SpawnSpaceshipFromSeed(roundData.spaceshipSeed);
        }

        private void SpawnSpaceshipFromSeed(SpaceshipSeed spaceshipSeed)
        {
            if (!spaceshipSeed.ShouldSpawn)
                return;
            if (spaceshipSeed.inBase)
            {
                spaceshipsSpawner.SpawnNew(basePlanet);
            }
            else
            {
                spaceshipsSpawner.SpawnNew(planetsSpawner.GetRandom().PlanetBase);
            }
        }

        public void RespawnFromData(GameSnapshot gameSnapshot)
        {
            basePlanet.SetActive(true);

            planetsSpawner.SpawnPlanets(gameSnapshot.planets);

            UnityEngine.Assertions.Assert.IsTrue(gameSnapshot.HasLoops);
            foreach (LoopData loop in gameSnapshot.loops)
            {
                PlanetBaseBehaviour planetBase = string.IsNullOrEmpty(loop.originId) ? basePlanet : planetsSpawner.GetById(loop.originId).PlanetBase;
                spaceshipsSpawner.SpawnFromLoop(loop.spaceshipId, planetBase, planetsSpawner.GetById(loop.destinationId));
            }
        }

        public void SetRoomBasedPose(Action onComplete)
        {
            MRUKRoom room = MRUK.Instance.GetCurrentRoom();
            Pose originPose = new Pose
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