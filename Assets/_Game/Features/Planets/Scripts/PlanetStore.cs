using UnityEngine;
using DigitalLove.Global;
using DigitalLove.Game.UI;

namespace DigitalLove.Game.Planets
{
    public class PlanetStore : MonoBehaviour
    {
        [SerializeField] private ResourcePanel lettersPanel;
        [SerializeField] private PlanetBody planetBody;
        [SerializeField] private FloatValue gameSpeed;

        private int lettersPerMinute;
        private int maxLetters;
        private int letters;
        private float countdown;

        public int Letters => letters;

        public void StartStoring(int lettersPerMinute, int maxLetters)
        {
            this.lettersPerMinute = lettersPerMinute;
            this.maxLetters = maxLetters;
            lettersPanel.Init(transform.position + transform.up * planetBody.RadiusOffset);
            letters = Random.Range(0, maxLetters / 2);
            lettersPanel.ShowLetters(letters, maxLetters);
            ResetCoundown();
        }

        public int PickLetters(int value)
        {
            int picked;
            picked = value > letters ? letters : value;
            letters -= picked;
            lettersPanel.ShowLetters(letters, maxLetters);
            return picked;
        }

        private void Update()
        {
            if (lettersPerMinute == 0)
                return;
            if (countdown <= 0)
            {
                IncreaseLetters(1);
                ResetCoundown();
            }
            countdown -= Time.deltaTime;
        }

        private void ResetCoundown()
        {
            countdown = 60f / lettersPerMinute / gameSpeed.value;
        }

        public void IncreaseLetters(int value)
        {
            letters += value;
            if (letters > maxLetters)
                letters = maxLetters;
            lettersPanel.ShowLetters(letters, maxLetters);
        }
    }
}
