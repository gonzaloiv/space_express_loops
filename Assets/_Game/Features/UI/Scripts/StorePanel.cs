using UnityEngine;

namespace DigitalLove.Game.UI
{
    public class StorePanel : MonoBehaviour
    {
        [SerializeField] private ResourcePanel lettersLabel;
        [SerializeField] private ResourcePanel moneyLabel;

        public void Show(int letters, int maxLetters, int money)
        {
            gameObject.SetActive(true);
            lettersLabel.ShowLetters(letters, maxLetters);
            moneyLabel.ShowLetters(money, 0);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}