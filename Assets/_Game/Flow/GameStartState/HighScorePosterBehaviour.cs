using DigitalLove.DataAccess;
using DigitalLove.Game.Persistence;
using DigitalLove.Global;
using DigitalLove.XR.MRUtilityKit;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class HighScorePosterBehaviour : MonoBehaviour
    {
        [SerializeField] private OnTheWallSpawner onTheWallSpawner;
        [SerializeField] private StringValue highScoreKey;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI scoreLabel;

        [Inject] private MemoryDataClient memoryDataClient;

        public void Show()
        {
            if (HasHighestScore(out int score))
            {
                scoreLabel.text = $"{score:N0}";
                if (!onTheWallSpawner.HasBeenSpawned)
                    onTheWallSpawner.Spawn();
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private bool HasHighestScore(out int score)
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            score = playerData.GetHighScoreMoney(highScoreKey);
            return score > 0;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}