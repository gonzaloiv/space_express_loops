using System;
using System.Collections.Generic;

namespace DigitalLove.Game.SpaceShaping
{
    public readonly struct PuzzleReadout : IEquatable<PuzzleReadout>
    {
        public float EntropySpent { get; }
        public float EntropyPending { get; }
        public float EntropyBudget { get; }
        public TrajectoryOutcome Outcome { get; }
        public bool HasResult { get; }
        public bool PredictedSolved { get; }
        public bool IsChecked { get; }
        public bool IsSolved { get; }
        public bool IsFailed { get; }
        public int Grade { get; }
        public int CommandCount { get; }

        private PuzzleReadout(
            SpaceShapingSession session, IReadOnlyList<TrajectoryResult> results, bool isChecked)
        {
            bool hasResult = results != null;
            EntropySpent = session.EntropyCommitted;
            EntropyPending = session.PendingEntropy;
            EntropyBudget = session.EntropyBudget;
            CommandCount = session.CommandCount;
            Outcome = hasResult ? results.Worst() : TrajectoryOutcome.Miss;
            HasResult = hasResult;
            PredictedSolved = hasResult && session.IsSolved(results);
            IsChecked = isChecked;
            IsSolved = isChecked && PredictedSolved;
            IsFailed = isChecked && hasResult && session.IsFailed(results);
            Grade = IsSolved ? session.Grade() : 0;
        }

        private PuzzleReadout(SpaceShapingSession session) : this(session, null, false)
        {
        }

        public static PuzzleReadout From(
            SpaceShapingSession session, TrajectoryResult result, bool isChecked) =>
            new(session, new[] { result }, isChecked);

        public static PuzzleReadout From(
            SpaceShapingSession session, IReadOnlyList<TrajectoryResult> results, bool isChecked) =>
            new(session, results, isChecked);

        public static PuzzleReadout Pending(SpaceShapingSession session) => new(session);

        public bool Equals(PuzzleReadout other) =>
            EntropySpent == other.EntropySpent
            && EntropyPending == other.EntropyPending
            && EntropyBudget == other.EntropyBudget
            && Outcome == other.Outcome
            && HasResult == other.HasResult
            && PredictedSolved == other.PredictedSolved
            && IsChecked == other.IsChecked
            && IsSolved == other.IsSolved
            && IsFailed == other.IsFailed
            && Grade == other.Grade;

        public override bool Equals(object obj) => obj is PuzzleReadout other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(
                HashCode.Combine(EntropySpent, EntropyPending, EntropyBudget, Outcome),
                HashCode.Combine(HasResult, PredictedSolved, IsChecked, IsSolved, IsFailed, Grade));
    }
}
