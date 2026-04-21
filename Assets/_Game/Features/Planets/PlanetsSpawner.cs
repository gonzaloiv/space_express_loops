using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class PlanetsSpawner : MonoBehaviour
    {
        [SerializeField] private PlanetBehaviour[] planets;

        public void Setup(PlanetSeed[] seeds)
        {
            for (int i = 0; i < seeds.Length; i++)
            {
                planets[i].Setup(seeds[0].radius.GetRandomValue());
            }
        }

        private void Start()
        {
            if (MRUK.Instance != null)
            {
                MRUK.Instance.SceneLoadedEvent.AddListener(HandleSceneLoaded);
                HandleSceneLoaded();
            }
        }

        private void HandleSceneLoaded()
        {
            MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
            if (currentRoom != null)
            {
                transform.position = currentRoom.GetRoomBounds().center;
            }
        }
    }
}