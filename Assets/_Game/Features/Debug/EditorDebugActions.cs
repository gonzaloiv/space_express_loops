using DigitalLove.Game.Planets;
using DigitalLove.Game.Spaceships;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Debug
{
    public class EditorDebugActions : MonoBehaviour
    {
        [SerializeField] private PlanetsSpawner planets;
        [SerializeField] private SpaceshipsSpawner spaceships;

        [SerializeField] private SpaceshipBehaviour spaceship;

        [Button]
        public void CreateRandomRoute()
        {
            SpaceshipBehaviour selected = spaceship != null ? spaceship : spaceships.Current;
            selected.SetRoute(planets.GetRandom());
        }
    }
}
