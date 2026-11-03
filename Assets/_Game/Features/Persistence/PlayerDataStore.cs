using System.Threading.Tasks;
using DigitalLove.DataAccess;

namespace DigitalLove.Game.Persistence
{
    public static class PlayerDataStore
    {
        public static async Task SaveAsync(
            PlayerData playerData,
            UnityPlayerDataClient unityPlayerDataClient)
        {
            if (unityPlayerDataClient.IsReady)
                await unityPlayerDataClient.Put(playerData);
        }
    }
}
