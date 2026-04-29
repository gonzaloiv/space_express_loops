using DigitalLove.DataAccess;
using Newtonsoft.Json;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.Persistence
{
    public class GameSnapshotClient : MonoBehaviour
    {
        [SerializeField] private float updateIntervalSecs = 5;

        [Header("Debug")]
        [SerializeField] private GameSnapshot gameSnapshot;
        [SerializeField] private PlayerData playerData;

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
            gameSnapshot = memoryDataClient.Get<GameSnapshot>();
            playerData = memoryDataClient.Get<PlayerData>();
            Cookie cookie = playerData.GetOrCreateCookie(GameSnapshot.CookieKey);
            cookie.metadata = JsonConvert.SerializeObject(gameSnapshot);
            await unityPlayerDataClient.Put(playerData);
            countdown = updateIntervalSecs;
            Debug.LogWarning("Player data updated!");
        }
    }
}