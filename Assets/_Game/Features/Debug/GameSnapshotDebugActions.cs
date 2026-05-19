using DigitalLove.DataAccess;
using DigitalLove.Game.Persistence;
using DigitalLove.Global;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.DebugActions
{
    [RequireComponent(typeof(EditorDebugContext))]
    public class GameSnapshotDebugActions : MonoBehaviour
    {
        [Inject] private MemoryDataClient memoryDataClient;

        [Button]
        public void Debug_AddLettersAndMoney()
        {
            GameSnapshot gameSnapshot = memoryDataClient.Get<GameSnapshot>();
            gameSnapshot.IncreaseLettersAndMoney(100, 1000);
        }
    }
}
