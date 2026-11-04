using System;
using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;

namespace DigitalLove.Game.PuzzleSelection
{
    public class RunCarousel
    {
        private readonly JobEntry[] jobs;
        private int focusedIndex;

        public RunCarousel(IReadOnlyList<JobEntry> entries, int focusedIndex)
        {
            jobs = Copy(entries);
            this.focusedIndex = jobs.Length > 0
                ? Math.Min(Math.Max(focusedIndex, 0), jobs.Length - 1)
                : -1;
        }

        public int Count => jobs.Length;

        public int FocusedIndex => focusedIndex;

        public JobEntry Focused => focusedIndex >= 0 ? jobs[focusedIndex] : default;

        public PuzzleData FocusedPuzzle => Focused.Puzzle;

        public bool CanEnter => Focused.State != JobState.Locked && Focused.Puzzle != null;

        public JobEntry this[int index] => IsInRange(index) ? jobs[index] : default;

        public bool Focus(int index)
        {
            if (!IsInRange(index) || index == focusedIndex)
                return false;
            if (jobs[index].State == JobState.Locked)
                return false;

            focusedIndex = index;
            return true;
        }

        public bool Move(int delta)
        {
            if (delta == 0 || jobs.Length == 0)
                return false;

            int target = Math.Min(Math.Max(focusedIndex + delta, 0), jobs.Length - 1);
            return Focus(target);
        }

        public static RunCarousel FromLevels(
            IReadOnlyList<PuzzleData> levels, RunProgress progress)
        {
            int count = levels != null ? levels.Count : 0;
            JobEntry[] entries = new JobEntry[count];
            for (int i = 0; i < count; i++)
                entries[i] = Entry(levels[i], i, progress);

            return new RunCarousel(entries, progress.CurrentIndex);
        }

        private static JobEntry Entry(PuzzleData puzzle, int index, RunProgress progress)
        {
            JobState state = StateAt(index, progress);
            return new JobEntry(index, puzzle, state, progress.Stars(index));
        }

        private static JobState StateAt(int index, RunProgress progress)
        {
            if (progress.IsSolved(index))
                return JobState.Solved;

            return index == progress.CurrentIndex ? JobState.Current : JobState.Locked;
        }

        private static JobEntry[] Copy(IReadOnlyList<JobEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return Array.Empty<JobEntry>();

            JobEntry[] result = new JobEntry[entries.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = entries[i];

            return result;
        }

        private bool IsInRange(int index) => index >= 0 && index < jobs.Length;
    }
}
