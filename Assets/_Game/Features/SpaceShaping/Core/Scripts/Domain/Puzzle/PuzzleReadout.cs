using System;
using System.Collections.Generic;
using UnityEngine;

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
        public bool IsFlying { get; }
        public bool IsSolved { get; }
        public bool IsFailed { get; }
        public bool NoGrade { get; }
        public int Grade { get; }
        public int CommandCount { get; }
        public bool HasLastEdit { get; }
        public float LastEditCost { get; }
        public Vector3 LastEditCentre { get; }

        private PuzzleReadout(
            SpaceShapingSession session, IReadOnlyList<TrajectoryResult> results,
            bool isChecked, bool isFlying)
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
            IsFlying = hasResult && isFlying;
            IsSolved = isChecked && PredictedSolved;
            IsFailed = isChecked && hasResult && session.IsFailed(results);
            NoGrade = session.NoGrade;
            Grade = IsSolved ? session.Grade() : 0;
            HasLastEdit = session.HasLastEdit;
            LastEditCost = session.LastEditCost;
            LastEditCentre = session.LastEditCentre;
        }

        private PuzzleReadout(SpaceShapingSession session) : this(session, null, false, false)
        {
        }

        public static PuzzleReadout From(
            SpaceShapingSession session, TrajectoryResult result, bool isChecked) =>
            new(session, new[] { result }, isChecked, false);

        public static PuzzleReadout From(
            SpaceShapingSession session, TrajectoryResult result, bool isChecked, bool isFlying) =>
            new(session, new[] { result }, isChecked, isFlying);

        public static PuzzleReadout From(
            SpaceShapingSession session, IReadOnlyList<TrajectoryResult> results, bool isChecked) =>
            new(session, results, isChecked, false);

        public static PuzzleReadout From(
            SpaceShapingSession session, IReadOnlyList<TrajectoryResult> results,
            bool isChecked, bool isFlying) =>
            new(session, results, isChecked, isFlying);

        public static PuzzleReadout Pending(SpaceShapingSession session) => new(session);

        public bool Equals(PuzzleReadout other) =>
            EntropySpent == other.EntropySpent
            && EntropyPending == other.EntropyPending
            && EntropyBudget == other.EntropyBudget
            && Outcome == other.Outcome
            && HasResult == other.HasResult
            && PredictedSolved == other.PredictedSolved
            && IsChecked == other.IsChecked
            && IsFlying == other.IsFlying
            && IsSolved == other.IsSolved
            && IsFailed == other.IsFailed
            && NoGrade == other.NoGrade
            && Grade == other.Grade
            && CommandCount == other.CommandCount
            && HasLastEdit == other.HasLastEdit
            && LastEditCost == other.LastEditCost
            && LastEditCentre == other.LastEditCentre;

        public override bool Equals(object obj) => obj is PuzzleReadout other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(
                HashCode.Combine(EntropySpent, EntropyPending, EntropyBudget, Outcome, NoGrade),
                HashCode.Combine(
                    HasResult, PredictedSolved, IsChecked, IsFlying, IsSolved, IsFailed, Grade),
                HashCode.Combine(CommandCount, HasLastEdit, LastEditCost, LastEditCentre));
    }
}
