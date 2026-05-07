using NUnit.Framework.Constraints;
using UnityEngine;

namespace DigitalLove.Game.UI
{
    public class StorePanel : MonoBehaviour
    {
        [SerializeField] private ResourcePanel lettersLabel;
        [SerializeField] private ResourcePanel moneyLabel;
        [SerializeField] private AudioSource increaseMoneyAudioSource;
        [SerializeField] private AudioSource decreaseMoneyAudioSource;

        private int previousMoney;

        public void Show(int letters, int maxLetters, int money)
        {
            gameObject.SetActive(true);
            lettersLabel.ShowLetters(letters, maxLetters);
            moneyLabel.ShowLetters(money, 0);
            PlayMoneyAudio(money);
        }

        private void PlayMoneyAudio(int money)
        {
            if (money > previousMoney)
            {
                increaseMoneyAudioSource.Play();
            }
            else
            {
                decreaseMoneyAudioSource.Play();
            }
            previousMoney = money;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}