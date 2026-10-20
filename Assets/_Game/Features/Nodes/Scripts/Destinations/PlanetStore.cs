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
        private float countdownSecs;
        private bool isStoring;

        private Action onPlanetFull;

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
            ResetCountdown();
        }

        public int PickAllLetters() => PickLetters(letters);

        public int PickLetters(int value)
        {
            int picked = value > letters ? letters : value;
            letters -= picked;
            lettersPanel.ShowLetters(letters, maxLetters);
            return picked;
        }

        private void Update()
        {
            if (!isStoring || lettersPerMinute <= 0)
                return;

            if (letters >= maxLetters)
            {
                isStoring = false;
                return;
            }

            if (countdownSecs <= 0)
            {
                IncreaseLetters(1);
                if (isStoring)
                    ResetCountdown();
            }

            countdownSecs -= Time.deltaTime;
        }

        private void ResetCountdown()
        {
            float speed = gameSpeed.value;
            countdownSecs = lettersPerMinute > 0 && speed > 0f
                ? 60f / (lettersPerMinute * speed)
                : float.PositiveInfinity;
        }

        public void IncreaseLetters(int value)
        {
            if (letters >= maxLetters)
                return;

            int previousLetters = letters;
            letters = Mathf.Min(letters + value, maxLetters);
            bool becameFull = letters >= maxLetters;
            bool enteredWarning = maxLetters - letters <= LettersBeforeFullToStartWarning
                && maxLetters - previousLetters > LettersBeforeFullToStartWarning;

            if (becameFull)
            {
                isStoring = false;
                onPlanetFull?.Invoke();
            }
            else
            {
                newLetterAudioSource.Play();
            }

            if (becameFull || enteredWarning)
                fullAudioSource.Play();

            bool showMaxValue = maxLetters - letters <= LettersBeforeFullToStartWarning;
            lettersPanel.ShowLetters(letters, maxLetters, showMaxValue);
        }

        public void ResetLetters()
        {
            letters = 0;
            lettersPanel.ShowLetters(letters, maxLetters);
            ResetCountdown();
        }
    }
}
