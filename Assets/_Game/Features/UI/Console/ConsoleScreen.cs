using System;
using System.Collections;
using System.Collections.Generic;
using DigitalLove.UI.Behaviours;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.UI.Console
{
    // The floating console screen: one line in the room at a time, in front of the player. It powers
    // on like a CRT, types each line in, holds it, swaps it for the next in the same place, and cuts
    // off. The launch boot drives it line by line; other moments (a fail restart) play a short message.
    public class ConsoleScreen : MonoBehaviour
    {
        [SerializeField] private SubtitlesLikeFollow view;
        [SerializeField] private Transform content;
        [Tooltip("The line a message is typed on.")]
        [SerializeField] private TMP_Text message;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float charsPerSec = 60f;
        [Tooltip("How long a finished line stays before the next one replaces it.")]
        [SerializeField, Min(0f)] private float lineHoldSecs = 0.4f;
        [Tooltip("How long a message's last line stays before the screen cuts off.")]
        [SerializeField, Min(0f)] private float messageHoldSecs = 0.6f;
        [SerializeField, Min(0f)] private float bootSecs = 0.18f;
        [SerializeField, Min(0f)] private float cutOffSecs = 0.12f;

        private Vector3 fullScale = Vector3.one;
        private bool hasFullScale;
        private TMP_Text current;

        // For the console's sound: the CRT power-on, characters typing, the cut.
        public event Action Booted;
        public event Action Typed;
        public event Action CuttingOff;

        public bool IsOpen { get; private set; }

        // Clears the screen and places it where the player is looking; Boot powers it on.
        public void Open()
        {
            if (!hasFullScale)
            {
                fullScale = content.localScale;
                hasFullScale = true;
            }

            IsOpen = true;
            current = null;
            message.gameObject.SetActive(false);
            content.gameObject.SetActive(true);
            view.ShowInCameraView();
        }

        public void ShowInCameraView() => view.ShowInCameraView();

        public IEnumerator Boot()
        {
            Booted?.Invoke();
            yield return CrtTransition.Boot(content, fullScale, bootSecs);
        }

        // Types a line on; a different line first holds the current one, then takes its place.
        public IEnumerator Type(TMP_Text line, int count)
        {
            if (line != current)
                yield return Swap(line);

            yield return TerminalTypewriter.RevealTo(line, count, charsPerSec, OnTyped);
        }

        public IEnumerator CutOff()
        {
            CuttingOff?.Invoke();
            yield return CrtTransition.CutOff(content, fullScale, cutOffSecs);
            Hide();
        }

        // Off at once, without the cut; the last line is cleared so the next opening starts empty.
        public void Hide()
        {
            if (current != null)
                current.gameObject.SetActive(false);

            content.gameObject.SetActive(false);
            if (hasFullScale)
                content.localScale = fullScale;
            current = null;
            IsOpen = false;
        }

        // Powers on, types each line in turn on the message line, holds, and cuts off.
        public void PlayMessage(IReadOnlyList<string> lines, Action onComplete)
        {
            StopAllCoroutines();
            Open();
            StartCoroutine(RunMessage(lines, onComplete));
        }

        private IEnumerator RunMessage(IReadOnlyList<string> lines, Action onComplete)
        {
            yield return Boot();
            for (int i = 0; i < lines.Count; i++)
                yield return TypeMessageLine(lines[i], i > 0);

            yield return new WaitForSecondsRealtime(messageHoldSecs);
            yield return CutOff();
            onComplete?.Invoke();
        }

        private IEnumerator TypeMessageLine(string text, bool holdFirst)
        {
            if (holdFirst)
                yield return new WaitForSecondsRealtime(lineHoldSecs);

            message.text = text;
            message.maxVisibleCharacters = 0;
            message.gameObject.SetActive(true);
            current = message;
            yield return TerminalTypewriter.RevealTo(message, int.MaxValue, charsPerSec, OnTyped);
        }

        private IEnumerator Swap(TMP_Text line)
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

        private void OnTyped() => Typed?.Invoke();
    }
}
