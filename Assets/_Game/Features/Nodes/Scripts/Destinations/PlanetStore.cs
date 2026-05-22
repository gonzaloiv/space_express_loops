using UnityEngine;
using DigitalLove.Global;
using DigitalLove.Game.UI;
using System;

namespace DigitalLove.Game.Nodes
{
    public class PlanetStore : MonoBehaviour
    {
        public const int LettersBeforeFullToStartWarning = 3;

        [SerializeField] private FloatValue gameSpeed;
        [SerializeField] private AudioSource fullAudioSource;
        [SerializeField] private AudioSource newLetterAudioSource;

        private ResourcePanel lettersPanel;
        private int lettersPerMinute;
        private int maxLetters;
        private int letters;
        private float countdown;
        private bool isStoring;

        private Action onPlanetFull;

        public int Letters => letters;

        public void PrepareStoring(
            ResourcePanel lettersPanel,
            int lettersPerMinute,
            int maxLetters,
            Action onPlanetFull)
        {
            this.lettersPanel = lettersPanel;
            this.lettersPerMinute = lettersPerMinute;
            this.maxLetters = maxLetters;
            this.onPlanetFull = onPlanetFull;
            letters = 0;
            isStoring = false;
            lettersPanel.ShowLetters(letters, maxLetters);
        }

        public void BeginStoring()
        {
            if (isStoring || lettersPerMinute <= 0)
                return;

            isStoring = true;
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
            if (!isStoring || lettersPerMinute == 0)
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
            if (letters >= maxLetters)
            {
                onPlanetFull?.Invoke();
                letters = maxLetters;
            }
            else
            {
                newLetterAudioSource.Play();
            }
            bool showMaxValue = maxLetters - letters <= LettersBeforeFullToStartWarning;
            if (showMaxValue)
                fullAudioSource.Play();
            lettersPanel.ShowLetters(letters, maxLetters, showMaxValue);
        }

        public void ResetLetters()
        {
            letters = 0;
            lettersPanel?.ShowLetters(letters, maxLetters);
            ResetCoundown();
        }
    }
}
