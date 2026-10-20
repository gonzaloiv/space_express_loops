using System;

namespace DigitalLove.Game.Persistence
{
    [Serializable]
    public class Store
    {
        public int letters;
        public int money;

        public void ResetLetters() => letters = 0;

        public void IncreaseLettersAndMoney(int lettersValue, int moneyValue)
        {
            letters += lettersValue;
            money += moneyValue;
        }
    }
}
