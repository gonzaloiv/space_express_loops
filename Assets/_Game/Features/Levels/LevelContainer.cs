using DigitalLove.Game.Planets;
using DigitalLove.Game.Spaceships;
using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class LevelContainer : MonoBehaviour
    {
        [SerializeField] private RoundSelector roundSelector;
        [SerializeField] private PlanetsSpawner planetsSpawner;
        [SerializeField] private SpaceshipsSpawner spaceshipSpawner;

        public void Setup()
        {
            SetPose();
            RoundData roundData = roundSelector.GetCurrent();
            planetsSpawner.Spawn(roundData.planetsSeed);
            spaceshipSpawner.Setup(planetsSpawner.BasePlanet);
        }

        private void SetPose()
        {
            Bounds roomBounds = MRUK.Instance.GetCurrentRoom().GetRoomBounds();
            transform.position = roomBounds.center;
        }
    }
}