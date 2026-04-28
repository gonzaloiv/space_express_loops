namespace DigitalLove.Levels
{
    public class GamePlay
    {
        private int letters;
        private int totalCollectedLetters;

        public int CurrentLetters => letters;

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
    }
}