namespace DigitalLove.Game.Persistence
{
    public readonly struct JobOpenResult
    {
        public static JobOpenResult None => new(false, -1, false);

        public readonly bool Changed;
        public readonly int SignpostIndex;
        public readonly bool ShowExitPrompt;

        public JobOpenResult(bool changed, int signpostIndex, bool showExitPrompt)
        {
            Changed = changed;
            SignpostIndex = signpostIndex;
            ShowExitPrompt = showExitPrompt;
        }
    }
}
