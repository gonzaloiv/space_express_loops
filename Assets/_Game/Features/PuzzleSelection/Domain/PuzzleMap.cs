using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class PuzzleMap
    {
        private readonly JobEntry[] entries;
        private readonly Vector3[] positions;
        private int selectedIndex;

        private PuzzleMap(JobEntry[] entries, Vector3[] positions, int selectedIndex)
        {
            this.entries = entries;
            this.positions = positions;
            this.selectedIndex = entries.Length > 0
                ? Mathf.Clamp(selectedIndex, 0, entries.Length - 1)
                : -1;
        }

        public int Count => entries.Length;

        public int SelectedIndex => selectedIndex;

        public JobEntry Selected => IsInRange(selectedIndex) ? entries[selectedIndex] : default;

        public PuzzleData SelectedPuzzle => Selected.Puzzle;

        public bool CanEnter =>
            IsInRange(selectedIndex)
            && Selected.State != JobState.Locked
            && Selected.Puzzle != null;

        public JobEntry this[int index] => IsInRange(index) ? entries[index] : default;

        public Vector3 Position(int index) => IsInRange(index) ? positions[index] : Vector3.zero;

        public bool Select(int index)
        {
            if (!IsInRange(index) || index == selectedIndex)
                return false;

            selectedIndex = index;
            return true;
        }

        public bool SelectNext(int delta)
        {
            if (delta == 0 || entries.Length == 0)
                return false;

            int target = Mathf.Clamp(selectedIndex + delta, 0, entries.Length - 1);
            return Select(target);
        }

        public int VisibleJobs(Vector3 focus, float radius, List<JobEntry> buffer)
        {
            buffer.Clear();
            float squaredRadius = Mathf.Max(radius, 0f);
            squaredRadius *= squaredRadius;
            for (int i = 0; i < entries.Length; i++)
            {
                if ((positions[i] - focus).sqrMagnitude <= squaredRadius)
                    buffer.Add(entries[i]);
            }

            return buffer.Count;
        }

        public static PuzzleMap FromLevels(
            IReadOnlyList<PuzzleData> levels, RunProgress progress, PuzzleMapLayout layout)
        {
            JobEntry[] entries = RunJobs.FromLevels(levels, progress);
            Vector3[] positions = new Vector3[entries.Length];
            for (int i = 0; i < entries.Length; i++)
                positions[i] = layout.At(i);

            return new PuzzleMap(entries, positions, progress.CurrentIndex);
        }

        private bool IsInRange(int index) => index >= 0 && index < entries.Length;
    }
}
