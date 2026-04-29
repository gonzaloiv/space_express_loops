using UnityEngine;
using System;
using DigitalLove.Game.Persistence;

namespace DigitalLove.Game.Levels
{
    public class RoundSelector : MonoBehaviour
    {
        [SerializeField] private RoundData[] rounds;

        private RoundData currentRound;

        public RoundData CurrentRound => currentRound;
        public int FormattedCurrentRoundIndex => Array.IndexOf(rounds, currentRound) + 1;

        public void SetCurrentRound(int index)
        {
            currentRound = rounds[index];
        }

        public bool IsRoundComplete(Store store)
        {
            return currentRound.lettersToComplete > 0 && store.letters >= currentRound.lettersToComplete;
        }
    }
}