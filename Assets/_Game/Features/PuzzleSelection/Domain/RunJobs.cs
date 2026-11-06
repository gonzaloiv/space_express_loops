using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;

namespace DigitalLove.Game.PuzzleSelection
{
    public static class RunJobs
    {
        public static JobEntry[] FromLevels(
            IReadOnlyList<PuzzleData> levels, RunProgress progress)
        {
            int count = levels != null ? levels.Count : 0;
            JobEntry[] entries = new JobEntry[count];
            for (int i = 0; i < count; i++)
                entries[i] = Entry(levels[i], i, progress);

            return entries;
        }

        private static JobEntry Entry(PuzzleData puzzle, int index, RunProgress progress)
        {
            JobState state = StateAt(index, progress);
            bool noGrade = puzzle != null && puzzle.noGrade;
            return new JobEntry(index, puzzle, state, progress.Stars(index), noGrade);
        }

        private static JobState StateAt(int index, RunProgress progress)
        {
            if (progress.IsSolved(index))
                return JobState.Solved;

            return index == progress.CurrentIndex ? JobState.Current : JobState.Locked;
        }
    }
}
