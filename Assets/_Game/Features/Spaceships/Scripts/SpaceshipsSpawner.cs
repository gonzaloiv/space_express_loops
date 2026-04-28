using DigitalLove.Game.Planets;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipsSpawner : MonoBehaviour
    {
        [SerializeField] private SpaceshipBehaviour spaceship;

        public SpaceshipBehaviour Current => spaceship;

        public void Spawn(BasePlanetBehaviour planet)
        {
            spaceship.Spawn(planet);
        }

        public void HideAll()
        {
            spaceship.Hide();
        }
    }
}