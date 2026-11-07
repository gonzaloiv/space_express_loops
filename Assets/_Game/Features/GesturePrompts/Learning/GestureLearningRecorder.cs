using DigitalLove.DataAccess;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.GesturePrompts
{
    // The panel only closes itself; this keeps the completion so the prompt never returns.
    public class GestureLearningRecorder : MonoBehaviour
    {
        [SerializeField] private GesturePromptSource source;

        [Inject] private MemoryDataClient memoryDataClient;
        [Inject] private UnityPlayerDataClient playerDataClient;

        private void OnEnable() => source.Completed += OnCompleted;

        private void OnDisable() => source.Completed -= OnCompleted;

        private void OnCompleted(GestureKind kind)
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            if (playerData == null)
            {
                Debug.LogError($"No player data; the learned {kind} gesture was not saved.");
                return;
            }

            GestureLearning.Save(playerData, kind, playerDataClient);
        }
    }
}