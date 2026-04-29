using System;
using System.Collections.Generic;
using DigitalLove.Game.Planets;
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

        public PlanetsSpawner PlanetsSpawner => planetsSpawner;
        public SpaceshipsSpawner SpaceshipsSpawner => spaceshipsSpawner;

        public void HideAll()
        {
            planetsSpawner.HideAll();
            spaceshipsSpawner.HideAll();
        }

        public void SpawnInitialRound(RoundData roundData, GameSnapshot gameSnapshot)
        {
            planetsSpawner.HideAll();
            planetsSpawner.SpawnBase(0);
            SpawnRound(roundData, gameSnapshot);
        }

        public void SpawnRound(RoundData roundData, GameSnapshot gameSnapshot)
        {
            if (roundData.newSpaceship)
                spaceshipsSpawner.SpawnNew(planetsSpawner.BasePlanet);
            List<PlanetData> roundPlanets = planetsSpawner.GeneratePlanetDataFromPlanetsSeed(roundData.planetsSeed);
            gameSnapshot.AddPlanets(roundPlanets);
            planetsSpawner.SpawnPlanets(gameSnapshot.planets);
        }

        public void RespawnFromData(GameSnapshot gameSnapshot)
        {
            planetsSpawner.SpawnPlanets(gameSnapshot.planets);
            UnityEngine.Assertions.Assert.IsTrue(gameSnapshot.HasLoops);
            foreach (LoopData loop in gameSnapshot.loops)
            {
                spaceshipsSpawner.SpawnFromLoop(loop.spaceshipId, planetsSpawner.BasePlanet, planetsSpawner.GetById(loop.destinationId));
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