namespace DigitalLove.Game.Flow
{
    // A one-shot intent written by the fail path and cleared by the next start or map entry, so a
    // restart skips Jim's intro without leaving a stale flag behind.
    internal readonly struct PuzzleRestart
    {
        public PuzzleRestart(bool value, int signpostIndex, bool showExitPrompt)
        {
            Value = value;
            SignpostIndex = signpostIndex;
            ShowExitPrompt = showExitPrompt;
        }

        public bool Value { get; }
        public int SignpostIndex { get; }
        public bool ShowExitPrompt { get; }
    }
}
