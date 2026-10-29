namespace DigitalLove.Game.SpaceShaping
{
    public enum PuzzleResultOutcome
    {
        Solved,
        Miss,
        Failed
    }

    public sealed class PuzzleResult
    {
        private PuzzleResult(PuzzleResultOutcome outcome, int grade, TrajectoryOutcome trajectory)
        {
            Outcome = outcome;
            Grade = grade;
            Trajectory = trajectory;
        }

        public PuzzleResultOutcome Outcome { get; }

        public int Grade { get; }

        public TrajectoryOutcome Trajectory { get; }

        public static PuzzleResult From(SpaceShapingSession session, TrajectoryResult result)
        {
            if (session.IsSolved(result))
                return new PuzzleResult(PuzzleResultOutcome.Solved, session.Grade(), result.Outcome);

            return session.IsFailed(result)
                ? new PuzzleResult(PuzzleResultOutcome.Failed, 0, result.Outcome)
                : new PuzzleResult(PuzzleResultOutcome.Miss, 0, result.Outcome);
        }
    }
}
