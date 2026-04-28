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
        [SerializeField] private SpaceshipsSpawner spaceshipsSpawner;

        public PlanetsSpawner PlanetsSpawner => planetsSpawner;
        public SpaceshipsSpawner SpaceshipsSpawner => spaceshipsSpawner;

        public void HideAll()
        {
            planetsSpawner.HideAll();
            spaceshipsSpawner.HideAll();
        }

        public void Spawn()
        {
            SetPose();
            RoundData roundData = roundSelector.GetCurrent();
            planetsSpawner.Spawn(roundData.planetsSeed);
            spaceshipsSpawner.Spawn(planetsSpawner.BasePlanet);
        }

        private void SetPose()
        {
            Bounds roomBounds = MRUK.Instance.GetCurrentRoom().GetRoomBounds();
            transform.position = roomBounds.center;
        }
    }
}