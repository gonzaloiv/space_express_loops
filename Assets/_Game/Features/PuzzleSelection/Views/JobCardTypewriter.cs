using System;
using System.Collections;
using DigitalLove.UI.Behaviours;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    internal static class JobCardTypewriter
    {
        public static IEnumerator Reveal(
            CardLines card, float typeCharsPerSec, float lineGapSecs, Action onLine)
        {
            TMP_Text[] lines = { card.Header, card.Client, card.Cargo, card.Manual };
            foreach (TMP_Text line in lines)
                line.maxVisibleCharacters = 0;

            foreach (TMP_Text line in lines)
            {
                yield return TerminalTypewriter.Reveal(line, typeCharsPerSec);
                if (typeCharsPerSec > 0f && lineGapSecs > 0f && line.textInfo.characterCount > 0)
                    yield return new WaitForSecondsRealtime(lineGapSecs);
                onLine?.Invoke();
            }
        }
    }

    internal readonly struct CardLines
    {
        public readonly TMP_Text Header;
        public readonly TMP_Text Client;
        public readonly TMP_Text Cargo;
        public readonly TMP_Text Manual;

        public CardLines(TMP_Text header, TMP_Text client, TMP_Text cargo, TMP_Text manual)
        {
            Header = header;
            Client = client;
            Cargo = cargo;
            Manual = manual;
        }
    }
}
