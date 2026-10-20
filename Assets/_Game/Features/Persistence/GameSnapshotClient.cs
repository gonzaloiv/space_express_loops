using DigitalLove.DataAccess;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.Persistence
{
    public class GameSnapshotClient : MonoBehaviour
    {
        [SerializeField] private float updateIntervalSecs = 5;

        [Inject] private MemoryDataClient memoryDataClient;
        [Inject] private UnityPlayerDataClient unityPlayerDataClient;

        private bool hasToUpdate;
        private float countdown;

        public void SetHasToUpdate()
        {
            hasToUpdate = true;
        }

        private void Update()
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0)
            {
                if (hasToUpdate)
                {
                    SavePlayerData();
                    hasToUpdate = false;
                }
                else
                {
                    countdown = updateIntervalSecs;
                }
            }
        }

        private async void SavePlayerData()
        {
            GameSnapshot gameSnapshot = memoryDataClient.Get<GameSnapshot>();
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            GameSnapshotStore.Save(gameSnapshot, playerData, memoryDataClient);
            await GameSnapshotStore.SavePlayerDataAsync(playerData, unityPlayerDataClient);
            countdown = updateIntervalSecs;
        }
    }
}