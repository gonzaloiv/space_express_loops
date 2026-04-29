using DigitalLove.Game.Planets;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using DigitalLove.Global;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipsSpawner : MonoBehaviour
    {
        [SerializeField] private List<SpaceshipBehaviour> spaceships;

        private IdCounter idCounter = new();

        public void SetOnLoopCreated(Action<LoopCreatedEventArgs> onLoopCreated)
        {
            foreach (SpaceshipBehaviour spaceship in spaceships)
            {
                spaceship.SetOnLoopCreated(onLoopCreated);
            }
        }

        public void SetOnLoopComplete(Action<int> onLoopComplete)
        {
            foreach (SpaceshipBehaviour spaceship in spaceships)
            {
                spaceship.SetOnLoopComplete(onLoopComplete);
            }
        }

        public void SpawnNew(BasePlanetBehaviour basePlanet)
        {
            SpaceshipBehaviour spaceship = GetOrInstantiate();
            spaceship.Spawn(idCounter.NextId, basePlanet);
        }

        public void SpawnFromLoop(string id, BasePlanetBehaviour basePlanet, PlanetBehaviour destinationPlanet)
        {
            SpaceshipBehaviour spaceship = GetOrInstantiate();
            spaceship.Spawn(id, basePlanet);
            spaceship.SetRoute(destinationPlanet);
        }

        private SpaceshipBehaviour GetOrInstantiate()
        {
            SpaceshipBehaviour spaceship = spaceships.FirstOrDefault(s => !s.IsActive);
            if (spaceship == null)
            {
                spaceship = Instantiate(spaceships[0], transform);
                spaceship.Hide();
                spaceships.Add(spaceship);
            }
            return spaceship;
        }

        public void HideAll()
        {
            foreach (SpaceshipBehaviour spaceship in spaceships)
            {
                spaceship.Hide();
            }
        }

        // ! DEBUG

        public SpaceshipBehaviour GetRandom()
        {
            return spaceships[UnityEngine.Random.Range(0, spaceships.Count)];
        }

        public List<SpaceshipBehaviour> GetAll() => spaceships.ToList();
    }
}