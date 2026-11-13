using DigitalLove.Game.SpaceShaping;

namespace DigitalLove.Game.PuzzleSelection
{
    public readonly struct JobEntry
    {
        public readonly int Index;
        public readonly PuzzleData Puzzle;
        public readonly JobState State;
        public readonly int Stars;
        public readonly bool NoGrade;

        public JobEntry(
            int index, PuzzleData puzzle, JobState state, int stars, bool noGrade)
        {
            Index = index;
            Puzzle = puzzle;
            State = state;
            Stars = stars;
            NoGrade = noGrade;
        }

        // Every graded job shows its stars: lit for what a solve earned, unlit while it is still to
        // complete or not reached yet. Only the ungraded tutorial shows none.
        public bool ShowsStars => !NoGrade;
    }
}
