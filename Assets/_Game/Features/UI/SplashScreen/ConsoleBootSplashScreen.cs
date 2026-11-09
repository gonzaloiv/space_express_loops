using System;
using System.Collections;
using DigitalLove.Casual.Flow.Setup;
using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using DigitalLove.Game.Persistence;
using DigitalLove.Global;
using DigitalLove.UI.Behaviours;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.UI.SplashScreen
{
    // The console boot: the dispatch lines type in front of the player, then the view cuts off like a
    // CRT.
    public class ConsoleBootSplashScreen : BaseSplashScreen
    {
        [Serializable]
        private class BootLine
        {
            [SerializeField] private TMP_Text text;
            [SerializeField] private bool freshOnly;

            public TMP_Text Text => text;
            public bool FreshOnly => freshOnly;
        }

        [SerializeField] private SubtitlesLikeFollow view;
        [SerializeField] private Transform content;
        [SerializeField] private BootLine[] lines;
        [SerializeField] private LevelSelector levelSelector;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float charsPerSec = 60f;
        [SerializeField, Min(0f)] private float lineGapSecs = 0.12f;
        [SerializeField, Min(0f)] private float holdSecs = 0.3f;
        [SerializeField, Min(0f)] private float bootSecs = 0.18f;
        [SerializeField, Min(0f)] private float cutOffSecs = 0.12f;

        [SerializeField] private DebugBool skip = new();

        [Inject] private MemoryDataClient memoryDataClient;

        private Action onComplete;

        public override void Play(Action onComplete)
        {
            if (skip.Value)
            {
                content.gameObject.SetActive(false);
                onComplete.Invoke();
                return;
            }

            this.onComplete = onComplete;
            content.gameObject.SetActive(true);
            view.ShowInCameraView();
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            Vector3 full = content.localScale;
            bool fresh = IsFresh();
            Prepare(fresh);

            yield return CrtTransition.Boot(content, full, bootSecs);
            yield return RevealLines(fresh);
            yield return new WaitForSecondsRealtime(holdSecs);
            yield return CrtTransition.CutOff(content, full, cutOffSecs);

            content.gameObject.SetActive(false);
            content.localScale = full;
            Complete();
        }

        private bool IsFresh()
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            return RunProgressCookies.Build(levelSelector, playerData).IsFresh;
        }

        private void Prepare(bool fresh)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                BootLine line = lines[i];
                line.Text.gameObject.SetActive(IsShown(line, fresh));
                line.Text.maxVisibleCharacters = 0;
            }
        }

        private IEnumerator RevealLines(bool fresh)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                BootLine line = lines[i];
                if (!IsShown(line, fresh))
                    continue;

                yield return Reveal(line.Text);

                if (lineGapSecs > 0f)
                    yield return new WaitForSecondsRealtime(lineGapSecs);
            }
        }

        private IEnumerator Reveal(TMP_Text text)
        {
            text.ForceMeshUpdate();
            int total = text.textInfo.characterCount;
            if (charsPerSec <= 0f)
            {
                text.maxVisibleCharacters = total;
                yield break;
            }

            if (total > 0)
                yield return RevealChars(text, total);
        }

        private IEnumerator RevealChars(TMP_Text text, int total)
        {
            float secsPerChar = 1f / charsPerSec;
            float elapsed = 0f;
            int shown = 0;
            while (shown < total)
            {
                elapsed += Time.unscaledDeltaTime;
                int next = Mathf.Min(total, Mathf.FloorToInt(elapsed / secsPerChar));
                if (next > shown)
                {
                    shown = next;
                    text.maxVisibleCharacters = shown;
                }

                yield return null;
            }
        }
        private static bool IsShown(BootLine line, bool fresh) => !line.FreshOnly || fresh;

        private void Complete()
        {
            Action callback = onComplete;
            onComplete = null;
            callback?.Invoke();
        }

        private void OnDisable() => onComplete = null;
    }
}
