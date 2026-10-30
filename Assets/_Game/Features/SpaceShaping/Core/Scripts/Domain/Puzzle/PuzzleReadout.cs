namespace DigitalLove.Game.SpaceShaping
{
    public readonly struct PuzzleReadout
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

        private PuzzleReadout(SpaceShapingSession session, TrajectoryResult result, bool isChecked)
        {
            bool solved = isChecked && session.IsSolved(result);
            EntropySpent = session.EntropyCommitted;
            EntropyPending = session.PendingEntropy;
            EntropyBudget = session.EntropyBudget;
            Outcome = result.Outcome;
            HasResult = true;
            PredictedSolved = session.IsSolved(result);
            IsChecked = isChecked;
            IsSolved = solved;
            IsFailed = isChecked && session.IsFailed(result);
            Grade = solved ? session.Grade() : 0;
            CommandCount = session.CommandCount;
        }

        private PuzzleReadout(SpaceShapingSession session)
        {
            EntropySpent = session.EntropyCommitted;
            EntropyPending = session.PendingEntropy;
            EntropyBudget = session.EntropyBudget;
            Outcome = TrajectoryOutcome.Miss;
            HasResult = false;
            PredictedSolved = false;
            IsChecked = false;
            IsSolved = false;
            IsFailed = false;
            Grade = 0;
            CommandCount = session.CommandCount;
        }

        public static PuzzleReadout From(
            SpaceShapingSession session, TrajectoryResult result, bool isChecked) =>
            new(session, result, isChecked);

        public static PuzzleReadout Pending(SpaceShapingSession session) => new(session);
    }
}
