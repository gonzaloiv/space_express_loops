using UnityEngine;
using DigitalLove.Game.Persistence;
using DigitalLove.Global;
using DigitalLove.Game.Spaceships;
using DigitalLove.Game.Planets;
using Reflex.Attributes;
using DigitalLove.DataAccess;

namespace DigitalLove.Game.UI
{
    public class StoreDependentUI : MonoBehaviour
    {
        [SerializeField] private SpaceshipsSpawner spaceshipsSpawner;
        [SerializeField] private PlanetsSpawner planetsSpawner;

        [Header("Economy")]
        [SerializeField] private IntValue routeEditionCost;
        [SerializeField] private IntValue planetColorCost;

        [Inject] private MemoryDataClient memoryDataClient;

        private GameSnapshot gameSnapshot;

        private void OnEnable()
        {
            gameSnapshot = memoryDataClient.Get<GameSnapshot>();
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

        private void OnDisable()
        {
            gameSnapshot.store.onUpdated -= DoUpdate;
        }
    }
}