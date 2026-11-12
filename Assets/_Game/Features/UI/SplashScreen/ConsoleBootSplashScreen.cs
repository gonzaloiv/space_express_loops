using System;
using System.Collections;
using DigitalLove.Casual.Flow.Setup;
using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using DigitalLove.Game.Persistence;
using DigitalLove.Game.UI.Console;
using DigitalLove.Global;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.UI.SplashScreen
{
    // The console boot, which is also the launch loader, played on the console screen: the header,
    // then one row per setup step as it runs, then the title lines once setup is done. Each line types
    // in, holds, and the next replaces it in the same place; the last one cuts off like a CRT.
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

        [SerializeField] private ConsoleScreen screen;
        [SerializeField] private TMP_Text header;
        [SerializeField] private BootChecklist checklist;
        [SerializeField] private BootLine[] lines;
        [SerializeField] private LevelSelector levelSelector;
        [SerializeField, Min(0f)] private float holdSecs = 0.6f;

        [SerializeField] private DebugBool skip = new();

        [Inject] private MemoryDataClient memoryDataClient;

        // For the boot's sound: a setup row getting its OK. The screen reports the rest.
        public event Action StepConfirmed;

        private bool playRequested;
        private Action onComplete;

        public override void Open()
        {
            if (skip.Value || screen.IsOpen)
                return;

            playRequested = false;
            HideLines();
            screen.Open();
            screen.PlaceEachLine = true;
            StartCoroutine(Run());
        }

        public override void BeginStep(SetupStep step) => checklist.Begin(step);

        public override void EndStep(SetupStep step)
        {
            checklist.End(step);
            // The boot opens before the headset is worn, so each new line re-places the screen until
            // the seat is placed; then the head is settled, the screen moves there once and stays.
            if (step != SetupStep.Space)
                return;

            screen.PlaceEachLine = false;
            if (screen.IsOpen)
                screen.ShowInCameraView();
        }

        public override void Play(Action onComplete)
        {
            if (skip.Value)
            {
                screen.Hide();
                onComplete.Invoke();
                return;
            }

            this.onComplete = onComplete;
            Open();
            playRequested = true;
        }

        public override void Close()
        {
            if (!screen.IsOpen)
                return;

            StopAllCoroutines();
            onComplete = null;
            StartCoroutine(screen.CutOff());
        }

        private IEnumerator Run()
        {
            yield return screen.Boot();
            yield return screen.Type(header, int.MaxValue);
            yield return TypeChecklist();
            yield return TypeTitle(IsFresh());
            yield return new WaitForSecondsRealtime(holdSecs);
            yield return screen.CutOff();
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
                yield return screen.Type(line, count);
                if (line.maxVisibleCharacters >= line.textInfo.characterCount)
                    StepConfirmed?.Invoke();
            }
        }

        private IEnumerator TypeTitle(bool fresh)
        {
            foreach (BootLine line in lines)
            {
                if (!line.FreshOnly || fresh)
                    yield return screen.Type(line.Text, int.MaxValue);
            }
        }

        private bool IsFresh()
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            return RunProgressCookies.Build(levelSelector, playerData).IsFresh;
        }

        private void HideLines()
        {
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
