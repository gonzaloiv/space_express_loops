namespace DigitalLove.Game.PuzzleSelection
{
    // The job's manual patch number in its chapter's play order, carried into the run so the
    // in-run contract matches the card the player selected.
    public readonly struct JobCardStamp
    {
        public readonly int ManualOrdinal;

        public JobCardStamp(int manualOrdinal)
        {
            ManualOrdinal = manualOrdinal;
        }
    }
}
