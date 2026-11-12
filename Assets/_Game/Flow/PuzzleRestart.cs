using DigitalLove.Localization;

namespace DigitalLove.Game.Flow
{
    // A one-shot intent written by the fail path and cleared by the next start or map entry, so a
    // restart skips Jim's intro without leaving a stale flag behind.
    internal readonly struct PuzzleRestart
    {
        private static readonly string[] ResetKeys =
        {
            "field_reset_spent",
            "field_reset_ok",
            "field_reset_full"
        };

        private const string JobOpenKey = "field_reset_job_open";

        public PuzzleRestart(bool value, int signpostIndex, bool showExitPrompt)
        {
            Value = value;
            SignpostIndex = signpostIndex;
            ShowExitPrompt = showExitPrompt;
        }

        public bool Value { get; }
        public int SignpostIndex { get; }
        public bool ShowExitPrompt { get; }

        // What the console types while the field restarts: the reset, then the job the fail opened.
        public string[] ConsoleLines()
        {
            bool opensJob = SignpostIndex >= 0;
            string[] lines = new string[ResetKeys.Length + (opensJob ? 1 : 0)];
            for (int i = 0; i < ResetKeys.Length; i++)
                lines[i] = LocalizationUtil.GetValue(ResetKeys[i]);

            if (opensJob)
                lines[ResetKeys.Length] = LocalizationUtil.GetValue(
                    JobOpenKey, (SignpostIndex + 1).ToString("D2"));

            return lines;
        }
    }
}
