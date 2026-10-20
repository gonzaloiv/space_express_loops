using System.Threading.Tasks;
using DigitalLove.DataAccess;
using Newtonsoft.Json;

namespace DigitalLove.Game.Persistence
{
    public static class GameSnapshotStore
    {
        public static GameSnapshot Load(MemoryDataClient memoryDataClient)
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            if (!playerData.HasCookie(GameSnapshot.CookieKey))
                return new GameSnapshot();

            Cookie cookie = playerData.GetCookieById(GameSnapshot.CookieKey);
            return GameSnapshot.FromCookieMetadata(cookie?.metadata);
        }

        public static void Save(
            GameSnapshot gameSnapshot,
            PlayerData playerData,
            MemoryDataClient memoryDataClient)
        {
            Cookie cookie = playerData.GetOrCreateCookie(GameSnapshot.CookieKey);
            cookie.metadata = JsonConvert.SerializeObject(gameSnapshot);
            memoryDataClient.Put(gameSnapshot);
            memoryDataClient.Put(playerData);
        }

        public static void Save(GameSnapshot gameSnapshot, MemoryDataClient memoryDataClient) =>
            Save(gameSnapshot, memoryDataClient.Get<PlayerData>(), memoryDataClient);

        public static async Task SavePlayerDataAsync(
            PlayerData playerData,
            UnityPlayerDataClient unityPlayerDataClient)
        {
            if (unityPlayerDataClient.IsReady)
                await unityPlayerDataClient.Put(playerData);
        }
    }
}
