using System;
using Newtonsoft.Json;

namespace DigitalLove.Game.Persistence
{
    [Serializable]
    public class Store
    {
        public int letters;
        public int money;

        [JsonIgnore] public Action onUpdated = () => { };

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
            onUpdated?.Invoke();
        }

        public bool CanAfford(int moneyValue) => money >= moneyValue;

        public bool TrySpendMoney(int moneyValue)
        {
            if (!CanAfford(moneyValue))
                return false;

            money -= moneyValue;
            onUpdated?.Invoke();
            return true;
        }

        public void SpendMoney(int moneyValue)
        {
            money -= moneyValue;
            onUpdated?.Invoke();
        }
    }
}