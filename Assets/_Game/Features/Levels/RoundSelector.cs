using UnityEngine;
using System;

namespace DigitalLove.Game.Levels
{
    public class RoundSelector : MonoBehaviour
    {
        [SerializeField] private RoundData[] rounds;

        private RoundData currentRound;

        public RoundData CurrentRound => currentRound;
        public int FormattedCurrentRoundIndex => Array.IndexOf(rounds, currentRound) + 1;
        public bool IsLastRound => rounds.Length == FormattedCurrentRoundIndex;
        public bool IsFirstRound => FormattedCurrentRoundIndex == 1;

        public void SetCurrentRound(int index)
        {
            if (index < 0)
                index = 0;
            if (index >= rounds.Length)
                index = rounds.Length - 1;
            currentRound = rounds[index];
        }
    }
}