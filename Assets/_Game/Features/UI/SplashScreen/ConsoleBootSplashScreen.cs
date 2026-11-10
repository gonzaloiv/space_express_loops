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
    // The console boot, which is also the launch loader. One line floats in the room at a time: the
    // header, then one row per setup step as it runs, then the title lines once setup is done. Each line
    // types in, holds, and the next replaces it in the same place; the last one cuts off like a CRT.
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
        [SerializeField] private TMP_Text header;
        [SerializeField] private BootChecklist checklist;
        [SerializeField] private BootLine[] lines;
        [SerializeField] private LevelSelector levelSelector;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float charsPerSec = 60f;
        [Tooltip("How long a finished line stays before the next one replaces it.")]
        [SerializeField, Min(0f)] private float lineHoldSecs = 0.4f;
        [SerializeField, Min(0f)] private float holdSecs = 0.6f;
        [SerializeField, Min(0f)] private float bootSecs = 0.18f;
        [SerializeField, Min(0f)] private float cutOffSecs = 0.12f;

        [SerializeField] private DebugBool skip = new();

        [Inject] private MemoryDataClient memoryDataClient;

        private Vector3 fullScale;
        private bool open;
        private bool playRequested;
        private TMP_Text current;
        private Action onComplete;

        public override void Open()
        {
            if (skip.Value || open)
                return;

            open = true;
            playRequested = false;
            fullScale = content.localScale;
            HideLines();
            content.gameObject.SetActive(true);
            view.ShowInCameraView();
            StartCoroutine(Run());
        }

        public override void BeginStep(SetupStep step) => checklist.Begin(step);

        public override void EndStep(SetupStep step)
        {
            checklist.End(step);
            // The boot opens before the headset is worn; once the seat is placed the head is settled,
            // so the line moves to where the player is actually looking.
            if (step == SetupStep.Space && open)
                view.ShowInCameraView();
        }

        public override void Play(Action onComplete)
        {
            if (skip.Value)
            {
                content.gameObject.SetActive(false);
                onComplete.Invoke();
                return;
            }

            this.onComplete = onComplete;
            Open();
            playRequested = true;
        }

        public override void Close()
        {
            if (!open)
                return;

            StopAllCoroutines();
            onComplete = null;
            StartCoroutine(CutOff());
        }

        private IEnumerator Run()
        {
            yield return CrtTransition.Boot(content, fullScale, bootSecs);
            yield return Type(header, int.MaxValue);
            yield return TypeChecklist();
            yield return TypeTitle(IsFresh());
            yield return new WaitForSecondsRealtime(holdSecs);
            yield return CutOff();
            Complete();
        }

        private IEnumerator TypeChecklist()
        {
            while (!playRequested || !checklist.IsIdle)
            {
                if (checklist.IsIdle)
                {
                    yield return null;
                    continue;
                }

                (TMP_Text line, int count) = checklist.Next();
                yield return Type(line, count);
            }
        }

        private IEnumerator TypeTitle(bool fresh)
        {
            foreach (BootLine line in lines)
            {
                if (!line.FreshOnly || fresh)
                    yield return Type(line.Text, int.MaxValue);
            }
        }

        // Types a line on; a different line first holds the current one, then takes its place.
        private IEnumerator Type(TMP_Text line, int count)
        {
            if (line != current)
            {
                if (current != null)
                {
                    yield return new WaitForSecondsRealtime(lineHoldSecs);
                    current.gameObject.SetActive(false);
                }

                line.maxVisibleCharacters = 0;
                line.gameObject.SetActive(true);
                current = line;
            }

            yield return TerminalTypewriter.RevealTo(line, count, charsPerSec);
        }

        private IEnumerator CutOff()
        {
            yield return CrtTransition.CutOff(content, fullScale, cutOffSecs);
            content.gameObject.SetActive(false);
            content.localScale = fullScale;
            open = false;
        }

        private bool IsFresh()
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            return RunProgressCookies.Build(levelSelector, playerData).IsFresh;
        }

        private void HideLines()
        {
            current = null;
            header.gameObject.SetActive(false);
            checklist.Hide();
            foreach (BootLine line in lines)
                line.Text.gameObject.SetActive(false);
        }

        private void Complete()
        {
            Action callback = onComplete;
            onComplete = null;
            callback?.Invoke();
        }

        private void OnDisable() => onComplete = null;
    }
}
