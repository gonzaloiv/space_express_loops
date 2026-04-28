using System;

namespace DigitalLove.Levels
{
    [Serializable]
    public class Store
    {
        public int letters;
        public int totalCollectedLetters;

        public void IncreaseLetters(int value)
        {
            letters += value;
            totalCollectedLetters += letters;
        }

        public void Reset()
        {
            letters = 0;
            totalCollectedLetters = 0;
        }

        public void CopyValues(Store other)
        {
            letters = other.letters;
            totalCollectedLetters = other.totalCollectedLetters;
        }
    }
}