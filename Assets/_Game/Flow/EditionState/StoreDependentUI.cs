using UnityEngine;
using DigitalLove.Game.Persistence;
using DigitalLove.Global;
using DigitalLove.Game.Spaceships;

namespace DigitalLove.Game.UI
{
    public class StoreDependentUI : MonoBehaviour
    {
        [SerializeField] private SpaceshipsSpawner spaceshipsSpawner;

        [Header("Economy")]
        [SerializeField] private IntValue routeEditionCost;

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
                if (gameSnapshot.store.money >= routeEditionCost.value)
                {
                    spaceship.RoutePanel.Show();
                }
                else
                {
                    spaceship.RoutePanel.Hide();
                }
            }
        }

        public void DoStop()
        {
            gameSnapshot.store.onUpdated -= DoUpdate;
        }
    }
}
