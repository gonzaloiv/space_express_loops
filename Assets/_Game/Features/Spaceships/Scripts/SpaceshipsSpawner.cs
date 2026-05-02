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

        public Action<LoopEventArgs> loopCreated = args => { };
        public Action<LoopEventArgs> loopEditionButtonClicked = args => { };
        public Action<LoopCompleteEventArgs> loopComplete = args => { };

        public void SpawnNew(PlanetBaseBehaviour basePlanet)
        {
            SpawnSpaceship(idCounter.NextId, basePlanet);
        }

        private SpaceshipBehaviour SpawnSpaceship(string id, PlanetBaseBehaviour basePlanet)
        {
            SpaceshipBehaviour spaceship = GetOrInstantiate();
            spaceship.Spawn(id, basePlanet);
            spaceship.SetOnLoopCreated(OnLoopCreated);
            spaceship.SetOnLoopComplete(OnLoopComplete);
            spaceship.SetOnLoopEditionButtonClicked(OnLoopEditionButtonClicked);
            return spaceship;
        }

        private void OnLoopCreated(LoopEventArgs args)
        {
            loopCreated(args);
        }

        private void OnLoopComplete(LoopCompleteEventArgs args)
        {
            loopComplete(args);
        }

        private void OnLoopEditionButtonClicked(LoopEventArgs args)
        {
            loopEditionButtonClicked(args);
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

        public void SpawnFromLoop(string id, PlanetBaseBehaviour basePlanet, PlanetBehaviour destinationPlanet)
        {
            SpaceshipBehaviour spaceship = SpawnSpaceship(id, basePlanet);
            spaceship.SetRoute(destinationPlanet);
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