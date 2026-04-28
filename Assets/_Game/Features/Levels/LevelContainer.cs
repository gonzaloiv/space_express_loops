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

        public void SpawnNew(RoundData roundData, Action<List<PlanetData>> onComplete)
        {
            SetRoomBasedPose(() =>
            {
                List<PlanetData> planetsData = planetsSpawner.GeneratePlanetDataFromPlanetsSeed(roundData.planetsSeed);
                planetsSpawner.Spawn(planetsData, 0);
                spaceshipsSpawner.Spawn(planetsSpawner.BasePlanet);
                onComplete.Invoke(planetsData);
            });
        }

        public void Respawn(GameSnapshot gameSnapshot)
        {
            SetRoomBasedPose(() =>
            {
                planetsSpawner.Spawn(gameSnapshot.planets, gameSnapshot.CurrentLetters);
                if (gameSnapshot.HasLoops)
                {
                    LoopData loopData = gameSnapshot.loops[0];
                    PlanetBehaviour destinationPlanet = PlanetsSpawner.GetById(loopData.destinationId);
                    spaceshipsSpawner.Respawn(loopData.spaceshipId, planetsSpawner.BasePlanet, destinationPlanet);
                }
                else
                {
                    spaceshipsSpawner.Spawn(planetsSpawner.BasePlanet);
                }
            });
        }

        private void SetRoomBasedPose(Action onComplete)
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