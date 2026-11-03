using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The entropy bar's shares: committed, in-flight preview, and last edit, as fractions of budget.
    public readonly struct EntropyBar
    {
        private const float MinBudget = 0.0001f;

        public readonly float Committed;
        public readonly float Pending;
        public readonly float LastEdit;
        public readonly bool Overflow;

        private EntropyBar(float committed, float pending, float lastEdit, bool overflow)
        {
            Committed = committed;
            Pending = pending;
            LastEdit = lastEdit;
            Overflow = overflow;
        }

        public float Remaining => 1f - Committed;

        public static EntropyBar From(PuzzleReadout readout)
        {
            float budget = Mathf.Max(readout.EntropyBudget, MinBudget);
            float committed = Mathf.Clamp01(readout.EntropySpent / budget);
            float remaining = 1f - committed;
            float pending = Mathf.Min(Mathf.Clamp01(readout.EntropyPending / budget), remaining);
            float lastEdit = readout.HasLastEdit
                ? Mathf.Min(Mathf.Clamp01(readout.LastEditCost / budget), remaining)
                : 0f;
            bool overflow = readout.EntropyBudget > 0f
                && readout.EntropySpent + readout.EntropyPending > readout.EntropyBudget;
            return new EntropyBar(committed, pending, lastEdit, overflow);
        }
    }
}
