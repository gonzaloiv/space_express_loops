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

        public bool ShowsStars => State == JobState.Solved && !NoGrade && Stars > 0;
    }
}
