using System;

namespace DigitalLove.Game.Persistence
{
    [Serializable]
    public class Store
    {
        public int letters;
        public int money;

        public void IncreaseLettersAndMoney(int lettersValue, int moneyValue)
        {
            letters += lettersValue;
            money += moneyValue;
        }

        public void SpendMoney(int moneyValue)
        {
            money -= moneyValue;
        }

        public void Reset()
        {
            letters = 0;
            money = 0;
        }

        public void CopyValues(Store other)
        {
            letters = other.letters;
            money = other.money;
        }
    }
}