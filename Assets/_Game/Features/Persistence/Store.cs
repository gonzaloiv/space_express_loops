using System;

namespace DigitalLove.Game.Persistence
{
    [Serializable]
    public class Store
    {
        public int letters;
        public int money;

        public Action onUpdated = () => { };

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

        // ? Updates

        public void IncreaseLettersAndMoney(int lettersValue, int moneyValue)
        {
            letters += lettersValue;
            money += moneyValue;
            onUpdated.Invoke();
        }

        public void SpendMoney(int moneyValue)
        {
            money -= moneyValue;
            onUpdated.Invoke();
        }
    }
}