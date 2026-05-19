using System.Collections.Generic;
using DigitalLove.Game.Planets;
using DigitalLove.Game.Spaceships;
using DigitalLove.Global;
using UnityEngine;
using DigitalLove.DataAccess;
using Reflex.Attributes;
using DigitalLove.Game.Persistence;

namespace DigitalLove.Game
{
    public class EditorDebugActions : MonoBehaviour
    {
        [SerializeField] private PlanetsSpawner planets;
        [SerializeField] private SpaceshipsSpawner spaceships;

        private List<string> excludedIds = new();

        [Inject] private MemoryDataClient memoryDataClient;

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
            spaceship.SetRoute(new[] { planet });
        }

        [Button]
        public void Debug_ClickEditionButtons()
        {
            List<SpaceshipBehaviour> toClick = spaceships.GetAll();
            foreach (SpaceshipBehaviour spaceship in toClick)
            {
                if (spaceship.HasRoute)
                    spaceship.Debug_InvokeOnLoopEditionButtonClicked();
            }
        }

        [Button]
        public void Debug_AddLettersAndMoney()
        {
            GameSnapshot gameSnapshot = memoryDataClient.Get<GameSnapshot>();
            gameSnapshot.IncreaseLettersAndMoney(100, 1000);
        }

        #region Route from references

        [Header("Route from references")]
        [SerializeField] private SpaceshipBehaviour routeSpaceship;
        [SerializeField] private PlanetBehaviour routePlanet;

        [Button]
        public void Debug_CreateRouteFromReferences()
        {
            if (routeSpaceship == null || routePlanet == null)
            {
                Debug.LogWarning("EditorDebugActions: Assign routeSpaceship and routePlanet.");
                return;
            }

            routeSpaceship.SetRoute(new[] { routePlanet });
        }

        #endregion
    }
}