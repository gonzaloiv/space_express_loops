namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleVerification
    {
        public TrajectoryResult Start;
        public TrajectoryResult SolvedRun;
        public float EntropySpent;
        public float StartMargin;
        public bool Solved;

        public TrajectoryOutcome StartOutcome => Start.Outcome;

        public TrajectoryOutcome SolvedOutcome => SolvedRun.Outcome;

        public float ShipTime => SolvedRun.ShipTime;

        public bool StartFails => Start.Outcome == TrajectoryOutcome.Miss;
    }
}
