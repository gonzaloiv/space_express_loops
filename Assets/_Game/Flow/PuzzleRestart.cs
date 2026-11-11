namespace DigitalLove.Game.Flow
{
    // A one-shot intent written by the fail path and cleared by the next start or map entry, so a
    // restart skips Jim's intro without leaving a stale flag behind.
    internal readonly struct PuzzleRestart
    {
        public PuzzleRestart(bool value) => Value = value;

        public bool Value { get; }
    }
}
