using DigitalLove.Levels;
using UnityEngine;
using System.Linq;
using System;

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
            if (CurrentRound == rounds.Last())
                return false;
            return currentRound.minLetters <= store.letters;
        }
    }
}