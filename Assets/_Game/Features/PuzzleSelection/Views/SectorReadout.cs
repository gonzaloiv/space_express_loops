using System.Collections;
using DigitalLove.Localization;
using DigitalLove.UI.Behaviours;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // The firm's reach and the world around it, in the map's field: the sector coverage count and the
    // wire line for it. A new solve types both in; any other visit shows them at once.
    public class SectorReadout : MonoBehaviour
    {
        [SerializeField] private TMP_Text coverage;
        [SerializeField] private TMP_Text wire;
        [SerializeField, Min(0f)] private float typeCharsPerSec = 40f;
        [SerializeField, Min(0f)] private float lineGapSecs = 0.3f;

        private Coroutine reveal;

        public void Show(int solved, int total, bool typed)
        {
            StopReveal();
            coverage.text = LocalizationUtil.GetValue(
                SectorWire.CoverageKey, solved.ToString("D2"), total.ToString("D2"));
            wire.text = LocalizationUtil.GetValue(SectorWire.Key(solved));
            gameObject.SetActive(true);

            if (typed)
            {
                reveal = StartCoroutine(Reveal());
                return;
            }

            coverage.maxVisibleCharacters = int.MaxValue;
            wire.maxVisibleCharacters = int.MaxValue;
        }

        public void Hide()
        {
            StopReveal();
            gameObject.SetActive(false);
        }

        private IEnumerator Reveal()
        {
            coverage.maxVisibleCharacters = 0;
            wire.maxVisibleCharacters = 0;
            yield return TerminalTypewriter.Reveal(coverage, typeCharsPerSec);
            yield return new WaitForSecondsRealtime(lineGapSecs);
            yield return TerminalTypewriter.Reveal(wire, typeCharsPerSec);
            reveal = null;
        }

        private void StopReveal()
        {
            if (reveal == null)
                return;

            StopCoroutine(reveal);
            reveal = null;
        }
    }
}
