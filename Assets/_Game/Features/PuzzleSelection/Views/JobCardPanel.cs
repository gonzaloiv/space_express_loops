using System;
using System.Collections;
using System.Globalization;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Localization;
using DigitalLove.UI.Behaviours;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DigitalLove.Game.PuzzleSelection
{
    public class JobCardPanel : MonoBehaviour
    {
        private const string TableName = "Levels";
        // The value column, in characters: the longest label ("CLIENT") plus a space.
        private const int LabelWidth = 7;
        // Labels and the manual's release line sit back so the contract's values read first.
        private const string Dim = "<alpha=#80>";
        private const string Full = "<alpha=#FF>";

        [SerializeField] private Transform cardRoot;
        [SerializeField] private TMP_Text header;
        [SerializeField] private TMP_Text client;
        [SerializeField] private TMP_Text cargo;
        [SerializeField] private TMP_Text manual;
        [Tooltip("Rule between the contract and the manual; shown only when the card has a manual.")]
        [SerializeField] private GameObject manualRule;
        [SerializeField, Min(0f)] private float typeCharsPerSec = 60f;
        [SerializeField, Min(0f)] private float bootSecs = 0.18f;
        [SerializeField, Min(0f)] private float closeSecs = 0.18f;
        [Tooltip("The card never gets shorter than this (canvas units); a longer contract grows it.")]
        [SerializeField, Min(0f)] private float minHeight = 300f;

        private Vector3? fullScale;
        private string headerTitle;
        private Coroutine bootRoutine;
        private Coroutine revealRoutine;
        private Coroutine closeRoutine;
        private float valueColumn;

        public event Action Opened;
        public event Action LineTyped;

        public void Show(PuzzleData puzzle, bool includeManual, int manualOrdinal)
        {
            Open(puzzle, includeManual, manualOrdinal, typed: true);
        }

        public void ShowInstant(PuzzleData puzzle, bool includeManual, int manualOrdinal)
        {
            Open(puzzle, includeManual, manualOrdinal, typed: false);
        }

        public void Hide()
        {
            StopReveal();
            StopBoot();
            if (!gameObject.activeSelf)
                return;

            closeRoutine = StartCoroutine(ShrinkOut());
        }

        private void Open(PuzzleData puzzle, bool includeManual, int manualOrdinal, bool typed)
        {
            bool wasActive = gameObject.activeSelf;
            StopClose();
            StopBoot();
            ApplyContent(puzzle, includeManual, manualOrdinal);
            gameObject.SetActive(true);
            FitHeight();
            if (typed)
                Opened?.Invoke();
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
        private void ApplyContent(PuzzleData puzzle, bool includeManual, int manualOrdinal)
        {
            header.text = $"{HeaderTitle}   {DateTime.Now:HH:mm}";
            string job = puzzle.JobNumberFor(Mathf.Max(1, manualOrdinal));
            client.text = $"{Row("JOB", job)}\n{Line("CLIENT", puzzle.ClientKey)}";
            cargo.text = Line("CARGO", puzzle.CargoKey);
            manual.text = includeManual
                ? ManualLine(puzzle, Mathf.Max(1, manualOrdinal))
                : string.Empty;
            manualRule.SetActive(manual.text.Length > 0);
        }

        // The manual is a release the tech team keeps patching: "MANUAL v1.0.3 (INITIAL DEPLOYMENT)",
        // then the entry on its own line. The release name shows on the chapter's first card only.
        private static string ManualLine(PuzzleData puzzle, int manualOrdinal)
        {
            string entry = LocalizationUtil.GetValue(tableName: TableName, key: puzzle.ManualKey);
            if (string.IsNullOrEmpty(entry))
                return string.Empty;

            string release = manualOrdinal == 1
                ? LocalizationUtil.GetValue(tableName: TableName, key: puzzle.ManualReleaseKey)
                : string.Empty;
            string version = $"MANUAL v{puzzle.ManualVersionFor(manualOrdinal)}";
            string heading = string.IsNullOrEmpty(release) ? version : $"{version} ({release})";
            return $"{Dim}{heading}{Full}\n{entry}";
        }

        // Grows the card to its text: the lines' layout reports its preferred height, and the card adds
        // the text area's inset around it. Runs once the card is active (an inactive layout measures
        // nothing); the text is set in full before the typewriter reveals it.
        private void FitHeight()
        {
            RectTransform card = (RectTransform)transform;
            RectTransform textArea = (RectTransform)header.rectTransform.parent;
            LayoutRebuilder.ForceRebuildLayoutImmediate(textArea);
            float needed = LayoutUtility.GetPreferredHeight(textArea) - textArea.sizeDelta.y;
            card.sizeDelta = new Vector2(card.sizeDelta.x, Mathf.Max(minHeight, needed));
        }

        private string HeaderTitle => headerTitle ??= header.text;

        // A label then its value at a fixed column; the indent is a hanging one, so a value that wraps
        // stays in its column instead of running back under the label. It closes before the row ends:
        // TMP fixes a line's start at the break, so the next row's label starts at the margin.
        private string Row(string name, string value) =>
            $"{Dim}{name}{Full}<indent={ValueColumn.ToString("0.#", CultureInfo.InvariantCulture)}px>{value}</indent>";

        // LabelWidth characters wide in the card's font, measured once the font reports a width.
        private float ValueColumn
        {
            get
            {
                if (valueColumn <= 0f)
                    valueColumn = client.GetPreferredValues(new string('M', LabelWidth)).x;
                return valueColumn;
            }
        }

        private string Line(string name, string key)
        {
            string value = LocalizationUtil.GetValue(tableName: TableName, key: key);
            return string.IsNullOrEmpty(value) ? string.Empty : Row(name, value);
        }

        private void StartReveal()
        {
            StopReveal();
            revealRoutine = StartCoroutine(
                JobCardTypewriter.Reveal(
                    new CardLines(header, client, cargo, manual),
                    typeCharsPerSec, () => LineTyped?.Invoke()));
        }

        private void RevealAll()
        {
            StopReveal();
            header.maxVisibleCharacters = int.MaxValue;
            client.maxVisibleCharacters = int.MaxValue;
            cargo.maxVisibleCharacters = int.MaxValue;
            manual.maxVisibleCharacters = int.MaxValue;
        }

        private void StopReveal() => StopRoutine(ref revealRoutine);

        private void StopClose()
        {
            if (closeRoutine == null)
                return;

            StopRoutine(ref closeRoutine);
            cardRoot.localScale = FullScale;
        }

        private IEnumerator BootIn()
        {
            yield return CrtTransition.Boot(cardRoot, FullScale, bootSecs);
            bootRoutine = null;
        }

        private void StopBoot() => StopRoutine(ref bootRoutine);

        private void StopRoutine(ref Coroutine routine)
        {
            if (routine == null)
                return;

            StopCoroutine(routine);
            routine = null;
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
        }
    }
}