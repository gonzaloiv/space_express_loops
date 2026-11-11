using System;
using System.Collections;
using DigitalLove.Audio;
using DigitalLove.Localization;
using DigitalLove.TTS;
using DigitalLove.UI.Behaviours;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class RunEndingView : MonoBehaviour
    {
        private const string TableName = "Levels";

        [SerializeField] private PuzzleMapView mapView;
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text total;
        [SerializeField] private TMP_Text console;
        [SerializeField] private GameObject title;
        [SerializeField] private TtsPresenter ttsPresenter;
        [SerializeField] private SynthCuePlayer cuePlayer;
        [SerializeField] private SynthPatch litCue;
        [SerializeField] private SynthCuePlayer systemPlayer;
        [SerializeField] private SynthPatch typeCue;
        [SerializeField] private string[] jimKeys;
        [SerializeField] private string consoleKey;
        [SerializeField, Min(0f)] private float nodeSecs = 0.14f;
        [SerializeField, Min(0f)] private float finishSecs = 0.35f;
        [SerializeField, Min(0f)] private float charsPerSec = 40f;
        [SerializeField, Min(0f)] private float lineHoldSecs = 0.3f;
        [SerializeField, Min(0f)] private float typeGapSecs = 0.06f;

        private Coroutine sequence;
        private Action onComplete;
        private bool playing;
        private bool revealed;
        private bool wasShowAll;

        private void Update()
        {
            if (playing && mapView.CurrentMap == null)
                Stop();
        }

        public void Play(PuzzleMap map, Action onComplete)
        {
            if (playing)
                StopSequence();

            this.onComplete = onComplete;
            wasShowAll = mapView.ShowAll;
            revealed = false;
            root.SetActive(true);
            total.gameObject.SetActive(false);
            console.text = string.Empty;
            title.SetActive(false);
            cuePlayer.Play(litCue);
            mapView.PlayRunEnding(map, nodeSecs, finishSecs, OnRevealed);
            playing = true;
            sequence = StartCoroutine(PlayLines(map));
        }

        public void Stop()
        {
            if (!playing)
                return;

            StopSequence();
            ttsPresenter.Stop();
            mapView.SetInteractive(true);
            mapView.SetShowAll(wasShowAll);
            onComplete = null;
        }

        private IEnumerator PlayLines(PuzzleMap map)
        {
            while (!revealed)
                yield return null;

            yield return WaitWhilePaused();
            total.text = $"{map.TotalStars} / {map.MaxStars}";
            total.gameObject.SetActive(true);
            title.SetActive(true);
            yield return WaitUnpaused(lineHoldSecs);
            yield return WaitWhilePaused();
            console.text = LocalizationUtil.GetValue(TableName, consoleKey);
            console.maxVisibleCharacters = 0;
            yield return TerminalTypewriter.RevealTo(
                console, int.MaxValue, charsPerSec, OnTyped);
            yield return WaitWhilePaused();
            sequence = null;
            ttsPresenter.Play(jimKeys, Finish);
        }

        private IEnumerator WaitWhilePaused()
        {
            while (Time.timeScale <= 0f)
                yield return null;
        }

        private IEnumerator WaitUnpaused(float secs)
        {
            float elapsed = 0f;
            while (elapsed < secs)
            {
                if (Time.timeScale > 0f)
                    elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void OnRevealed() => revealed = true;

        private void OnTyped() => systemPlayer.PlayTick(typeCue, typeGapSecs);

        private void Finish()
        {
            StopSequence();
            mapView.SetInteractive(true);
            mapView.SetShowAll(wasShowAll);
            Action callback = onComplete;
            onComplete = null;
            callback?.Invoke();
        }

        private void StopSequence()
        {
            playing = false;
            mapView.StopRunEnding();
            if (sequence != null)
            {
                StopCoroutine(sequence);
                sequence = null;
            }

            root.SetActive(false);
        }

        private void OnDisable() => Stop();
    }
}
