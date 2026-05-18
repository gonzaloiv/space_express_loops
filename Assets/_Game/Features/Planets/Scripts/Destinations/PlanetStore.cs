using UnityEngine;
using DigitalLove.Global;
using DigitalLove.Game.UI;
using System;

namespace DigitalLove.Game.Planets
{
    public class PlanetStore : MonoBehaviour
    {
        [SerializeField] private ResourcePanel lettersPanel;
        [SerializeField] private FloatValue gameSpeed;
        [SerializeField] private AudioSource fullAudioSource;
        [SerializeField] private AudioSource newLetterAudioSource;

        private int lettersPerMinute;
        private int maxLetters;
        private int letters;
        private float countdown;

        private Action planetFull;

        public int Letters => letters;

        public void StartStoring(int lettersPerMinute, int maxLetters, Action planetFull)
        {
            this.lettersPerMinute = lettersPerMinute;
            this.maxLetters = maxLetters;
            this.planetFull = planetFull;
            letters = 0;
            lettersPanel.ShowLetters(letters, maxLetters);
            ResetCoundown();
        }

        public int PickAllLetters() => PickLetters(letters);

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
            countdown = 60f / (lettersPerMinute * gameSpeed.value);
        }

        public void IncreaseLetters(int value)
        {
            letters += value;
            if (letters > maxLetters)
            {
                fullAudioSource.Play();
                planetFull?.Invoke();
                letters = maxLetters;
            }
            else
            {
                newLetterAudioSource.Play();
            }
            lettersPanel.ShowLetters(letters, maxLetters, letters == maxLetters);
        }
    }
}
