using System;
using System.Collections;
using DigitalLove.UI.Behaviours;
using TMPro;

namespace DigitalLove.Game.PuzzleSelection
{
    internal static class JobCardTypewriter
    {
        // The contract lands at once; only the manual types, and its typing ticks.
        public static IEnumerator Reveal(
            CardLines card, float typeCharsPerSec, Action onLine)
        {
            card.Header.maxVisibleCharacters = int.MaxValue;
            card.Client.maxVisibleCharacters = int.MaxValue;
            card.Cargo.maxVisibleCharacters = int.MaxValue;
            card.Manual.maxVisibleCharacters = 0;
            yield return TerminalTypewriter.RevealTo(
                card.Manual, int.MaxValue, typeCharsPerSec, onLine);
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
