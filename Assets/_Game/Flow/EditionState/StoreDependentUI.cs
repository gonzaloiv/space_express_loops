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

        private GameSnapshot gameSnapshot;
        private IntValue planetColorChangeCost;

        public void DoStart(GameSnapshot gameSnapshot, IntValue planetColorChangeCost)
        {
            this.gameSnapshot = gameSnapshot;
            this.planetColorChangeCost = planetColorChangeCost;
            gameSnapshot.store.onUpdated += DoUpdate;
        }

        public void DoUpdate()
        {
            foreach (SpaceshipBehaviour spaceship in spaceshipsSpawner.All)
            {
                if (gameSnapshot.store.money >= routeEditionCost.value)
                {
                    spaceship.RoutePanel.Show();
                }
                else
                {
                    spaceship.RoutePanel.Hide();
                }
            }
            int colorCost = planetColorChangeCost.value;
            foreach (PlanetBehaviour planet in planetsSpawner.All)
            {
                if (!planet.IsActive)
                    continue;

                planet.SetColorButtonPanel.SetActive(gameSnapshot.store.CanAfford(colorCost), colorCost);
            }
        }

        public void DoStop()
        {
            gameSnapshot.store.onUpdated -= DoUpdate;
        }
    }
}