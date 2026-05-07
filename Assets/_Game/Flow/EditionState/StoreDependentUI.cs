using UnityEngine;
using DigitalLove.Game.Persistence;
using DigitalLove.Global;
using DigitalLove.Game.Spaceships;
using DigitalLove.Game.Planets;

namespace DigitalLove.Game.UI
{
    public class StoreDependentUI : MonoBehaviour
    {
        [SerializeField] private SpaceshipsSpawner spaceshipsSpawner;
        [SerializeField] private PlanetsSpawner planetsSpawner;

        [Header("Economy")]
        [SerializeField] private IntValue routeEditionCost;
        [SerializeField] private IntValue planetColorCost;

        private GameSnapshot gameSnapshot;

        public void DoStart(GameSnapshot gameSnapshot)
        {
            this.gameSnapshot = gameSnapshot;
            gameSnapshot.store.onUpdated += DoUpdate;
        }

        public void DoUpdate()
        {
            foreach (SpaceshipBehaviour spaceship in spaceshipsSpawner.All)
            {
                spaceship.RoutePanel.SetEditionButtonActive(gameSnapshot.store.money >= routeEditionCost.value);
            }
            foreach (PlanetBehaviour planet in planetsSpawner.All)
            {
                planet.SetColorButtonPanel.SetActive(gameSnapshot.store.money >= planetColorCost.value);
            }
        }

        public void DoStop()
        {
            gameSnapshot.store.onUpdated -= DoUpdate;
        }
    }
}