using DigitalLove.Game.Planets;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipsSpawner : MonoBehaviour
    {
        [SerializeField] private SpaceshipBehaviour spaceship;

        private IdCreator idCreator = new();

        public SpaceshipBehaviour Current => spaceship;

        public void Spawn(BasePlanetBehaviour basePlanet)
        {
            spaceship.Spawn(idCreator.NextId, basePlanet);
        }

        public void Respawn(string id, BasePlanetBehaviour basePlanet, PlanetBehaviour destinationPlanet)
        {
            spaceship.Spawn(id, basePlanet);
            spaceship.SetRoute(destinationPlanet);
        }

        public void HideAll()
        {
            spaceship.Hide();
        }
    }
}