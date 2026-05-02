using System.Collections.Generic;
using DigitalLove.Game.Planets;
using DigitalLove.Game.Spaceships;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game
{
    public class EditorDebugActions : MonoBehaviour
    {
        [SerializeField] private PlanetsSpawner planets;
        [SerializeField] private SpaceshipsSpawner spaceships;

        private List<string> excludedIds = new();

        [Button]
        public void Debug_CreateRandomRoute()
        {
            excludedIds.Clear();
            FillRoute(spaceships.GetRandom());
        }

        [Button]
        public void Debug_FillRoutes()
        {
            excludedIds.Clear();
            List<SpaceshipBehaviour> toFill = spaceships.GetAll();
            foreach (SpaceshipBehaviour spaceship in toFill)
            {
                FillRoute(spaceship);
            }
        }

        private void FillRoute(SpaceshipBehaviour spaceship)
        {
            PlanetBehaviour planet = planets.GetRandom(excludedIds);
            excludedIds.Add(planet.Id);
            spaceship.SetRoute(planet);
        }

        [Button]
        public void Debug_RemoveRandomLoop()
        {
            SpaceshipBehaviour spaceship = spaceships.GetRandom();
            spaceship.Debug_InvokeOnLoopEditionButtonClicked();
        }
    }
}