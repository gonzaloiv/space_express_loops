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
        public bool IsLastRound => rounds.Length == FormattedCurrentRoundIndex;
        public int TotalLettersToComplete
        {
            get
            {
                int lettersToComplete = 0;
                for (int i = 0; i < Array.IndexOf(rounds, currentRound); i++)
                {
                    lettersToComplete += rounds[i].lettersToComplete;
                }
                return lettersToComplete;
            }
        }

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