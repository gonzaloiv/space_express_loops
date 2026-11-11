using System;
using System.Collections;
using DigitalLove.Audio;
using DigitalLove.Localization;
using DigitalLove.UI.Behaviours;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The short terminal readout a fail restart types beside the entropy post. It never blocks input:
    // a pinch cancels it and the post returns to the real budget.
    public class FieldResetReadout : MonoBehaviour
    {
        private const string JobOpenKey = "field_reset_job_open";

        [SerializeField] private SpaceShapingView view;
        [SerializeField] private FieldEdgeMeter meter;
        [SerializeField] private Transform readoutRoot;
        [SerializeField] private TMP_Text line;
        [SerializeField] private SynthCuePlayer systemPlayer;
        [SerializeField] private SynthPatch typeCue;
        [SerializeField, Min(1f)] private float charsPerSec = 45f;
        [SerializeField, Min(0f)] private float typeGapSecs = 0.06f;
        [SerializeField, Min(0f)] private float holdSecs = 0.35f;
        [SerializeField, Min(0f)] private float textHeight = 0.02f;
        [SerializeField, Min(0f)] private float textInset = 0.03f;
        [SerializeField] private string[] lineKeys =
        {
            "field_reset_spent",
            "field_reset_ok",
            "field_reset_full"
        };

        private bool running;
        private int signpostIndex = -1;

        public event Action Ended;

        private void OnEnable() => view.PinchCommitted += Cancel;

        private void OnDisable() => view.PinchCommitted -= Cancel;

        public void Play(int signpostIndex = -1)
        {
            this.signpostIndex = signpostIndex;
            StopAllCoroutines();
            readoutRoot.gameObject.SetActive(true);
            line.text = BuildText();
            line.maxVisibleCharacters = 0;
            line.ForceMeshUpdate();
            meter.ShowReset(0f);
            running = true;
            StartCoroutine(Run());
        }

        private string BuildText()
        {
            string text = string.Empty;
            for (int i = 0; i < lineKeys.Length; i++)
            {
                if (i > 0)
                    text += "\n";
                text += LocalizationUtil.GetValue(lineKeys[i]);
            }

            if (signpostIndex >= 0)
                text += "\n" + LocalizationUtil.GetValue(
                    JobOpenKey, (signpostIndex + 1).ToString("D2"));

            return text;
        }

        private IEnumerator Run()
        {
            int total = line.textInfo.characterCount;
            int lastLineStart = line.text.LastIndexOf('\n') + 1;
            float lastLineSecs = Mathf.Max((total - lastLineStart) / charsPerSec, 0.01f);
            StartCoroutine(Refill(lastLineStart / charsPerSec, lastLineSecs));
            yield return TerminalTypewriter.RevealTo(line, int.MaxValue, charsPerSec, OnTyped);
            meter.ShowReset(1f);
            yield return new WaitForSecondsRealtime(holdSecs);
            Finish();
        }

        // The post refills while the last line types.
        private IEnumerator Refill(float delaySecs, float duration)
        {
            yield return new WaitForSecondsRealtime(delaySecs);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                meter.ShowReset(Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            meter.ShowReset(1f);
        }

        private void OnTyped() => systemPlayer.PlayTick(typeCue, typeGapSecs);

        private void Update()
        {
            if (running && view.IsScaling)
                Cancel();
        }

        private void LateUpdate()
        {
            if (!readoutRoot.gameObject.activeSelf)
                return;

            float half = view.FieldHalf;
            Vector3 edge = FieldFrame.OnMeterEdge(half, -half + textHeight, textInset);
            // The readout is a child of the meter view, which already sits at the field centre.
            readoutRoot.localPosition = edge;
        }

        private void Cancel()
        {
            if (!running)
                return;

            StopAllCoroutines();
            Finish();
        }

        private void Finish()
        {
            running = false;
            readoutRoot.gameObject.SetActive(false);
            meter.ClearReset();
            Ended?.Invoke();
        }
    }
}
