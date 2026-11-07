using System;
using System.Collections;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Localization;
using DigitalLove.UI.Behaviours;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class JobCardPanel : MonoBehaviour
    {
        private const string TableName = "Levels";
        private const int LabelWidth = 11;

        [SerializeField] private Transform cardRoot;
        [SerializeField] private TMP_Text header;
        [SerializeField] private TMP_Text client;
        [SerializeField] private TMP_Text cargo;
        [SerializeField] private TMP_Text manual;
        [SerializeField] private GameObject cursor;
        [SerializeField, Min(0f)] private float typeCharsPerSec = 40f;
        [SerializeField, Min(0f)] private float lineGapSecs = 0.12f;
        [SerializeField, Min(0f)] private float bootSecs = 0.18f;
        [SerializeField, Min(0f)] private float closeSecs = 0.18f;
        [SerializeField, Min(0.05f)] private float cursorBlinkSecs = 0.5f;

        private Vector3? fullScale;
        private string headerTitle;
        private Coroutine bootRoutine;
        private Coroutine revealRoutine;
        private Coroutine closeRoutine;
        private float cursorElapsed;

        public void Show(PuzzleData puzzle, bool includeManual = true)
        {
            Open(puzzle, includeManual, typed: true);
        }

        public void ShowInstant(PuzzleData puzzle, bool includeManual = true)
        {
            Open(puzzle, includeManual, typed: false);
        }

        public void Hide()
        {
            StopReveal();
            StopBoot();
            if (!gameObject.activeSelf)
                return;

            closeRoutine = StartCoroutine(ShrinkOut());
        }

        private void Update()
        {
            cursorElapsed += Time.unscaledDeltaTime;
            bool lit = cursorElapsed % (cursorBlinkSecs * 2f) < cursorBlinkSecs;
            cursor.SetActive(lit && gameObject.activeSelf);
        }

        private void Open(PuzzleData puzzle, bool includeManual, bool typed)
        {
            bool wasActive = gameObject.activeSelf;
            StopClose();
            StopBoot();
            ApplyContent(puzzle, includeManual);
            gameObject.SetActive(true);
            if (typed || !wasActive)
                StartBoot();
            else
                cardRoot.localScale = FullScale;

            if (typed)
                StartReveal();
            else
                RevealAll();
        }

        private Vector3 FullScale => fullScale ??= cardRoot.localScale;

        private void StartBoot()
        {
            cardRoot.localScale = CrtTransition.Flat(FullScale);
            bootRoutine = StartCoroutine(BootIn());
        }

        // The header carries the console's clock: the player's own local time, usually late.
        private void ApplyContent(PuzzleData puzzle, bool includeManual)
        {
            header.text = $"{HeaderTitle}   {DateTime.Now:HH:mm}";
            client.text = Line(Label("CLIENT"), puzzle.ClientKey);
            cargo.text = Line(Label("CARGO"), puzzle.CargoKey);
            manual.text = includeManual
                ? Line(Label($"MANUAL r{puzzle.ManualRevision}"), puzzle.ManualKey)
                : string.Empty;
        }

        private string HeaderTitle => headerTitle ??= header.text;

        private static string Label(string name) => name.PadRight(LabelWidth);

        private static string Line(string label, string key)
        {
            string value = LocalizationUtil.GetValue(tableName: TableName, key: key);
            return string.IsNullOrEmpty(value) ? string.Empty : label + value;
        }

        private void StartReveal()
        {
            StopReveal();
            revealRoutine = StartCoroutine(
                JobCardTypewriter.Reveal(
                    header, client, cargo, manual, typeCharsPerSec, lineGapSecs));
        }

        private void RevealAll()
        {
            StopReveal();
            header.maxVisibleCharacters = int.MaxValue;
            client.maxVisibleCharacters = int.MaxValue;
            cargo.maxVisibleCharacters = int.MaxValue;
            manual.maxVisibleCharacters = int.MaxValue;
        }

        private void StopReveal()
        {
            if (revealRoutine == null)
                return;

            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        private void StopClose()
        {
            if (closeRoutine == null)
                return;

            StopCoroutine(closeRoutine);
            closeRoutine = null;
            cardRoot.localScale = FullScale;
        }

        private IEnumerator BootIn()
        {
            yield return CrtTransition.Boot(cardRoot, FullScale, bootSecs);
            bootRoutine = null;
        }

        private void StopBoot()
        {
            if (bootRoutine == null)
                return;

            StopCoroutine(bootRoutine);
            bootRoutine = null;
        }

        private IEnumerator ShrinkOut()
        {
            yield return CrtTransition.CutOff(cardRoot, FullScale, closeSecs);
            cardRoot.localScale = FullScale;
            closeRoutine = null;
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            StopReveal();
            StopBoot();
            closeRoutine = null;
            if (fullScale.HasValue)
                cardRoot.localScale = fullScale.Value;
            cursor.SetActive(false);
        }
    }
}