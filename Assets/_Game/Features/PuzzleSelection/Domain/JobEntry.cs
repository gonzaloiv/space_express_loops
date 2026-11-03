using DigitalLove.Game.SpaceShaping;

namespace DigitalLove.Game.PuzzleSelection
{
    public readonly struct JobEntry
    {
        public readonly int Index;
        public readonly PuzzleData Puzzle;
        public readonly JobState State;
        public readonly int Stars;

        public JobEntry(int index, PuzzleData puzzle, JobState state, int stars)
        {
            Index = index;
            Puzzle = puzzle;
            State = state;
            Stars = stars;
        }
    }
}
