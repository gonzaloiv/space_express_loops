using System.Collections;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    internal static class JobCardTypewriter
    {
        public static IEnumerator Reveal(
            TMP_Text header, TMP_Text client, TMP_Text cargo, TMP_Text manual,
            float typeCharsPerSec, float lineGapSecs)
        {
            TMP_Text[] lines = { header, client, cargo, manual };
            foreach (TMP_Text line in lines)
                line.maxVisibleCharacters = 0;

            foreach (TMP_Text line in lines)
                yield return RevealLine(line, typeCharsPerSec, lineGapSecs);
        }

        private static IEnumerator RevealLine(
            TMP_Text line, float typeCharsPerSec, float lineGapSecs)
        {
            line.ForceMeshUpdate();
            int total = line.textInfo.characterCount;
            if (total <= 0)
                yield break;

            if (typeCharsPerSec <= 0f)
            {
                line.maxVisibleCharacters = total;
                yield break;
            }

            yield return RevealChars(line, total, typeCharsPerSec);
            if (lineGapSecs > 0f)
                yield return new WaitForSecondsRealtime(lineGapSecs);
        }

        private static IEnumerator RevealChars(
            TMP_Text line, int total, float typeCharsPerSec)
        {
            float secsPerChar = 1f / typeCharsPerSec;
            float elapsed = 0f;
            int shown = 0;
            while (shown < total)
            {
                elapsed += Time.unscaledDeltaTime;
                int next = Mathf.Min(total, Mathf.FloorToInt(elapsed / secsPerChar));
                if (next == shown)
                {
                    yield return null;
                    continue;
                }

                shown = next;
                line.maxVisibleCharacters = shown;
                yield return null;
            }
        }
    }
}
