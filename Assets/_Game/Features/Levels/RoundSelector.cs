using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class RoundSelector : MonoBehaviour
    {
        [SerializeField] private RoundData[] rounds;

        public RoundData GetCurrent()
        {
            return rounds[0];
        }
    }
}