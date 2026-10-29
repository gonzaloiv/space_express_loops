namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleVerification
    {
        public TrajectoryResult Start;
        public TrajectoryResult SolvedRun;
        public PuzzleZone[] Solution;
        public float EntropySpent;
        public float StartMargin;
        public bool StartFails;
        public bool Solved;

        public TrajectoryOutcome StartOutcome => Start.Outcome;

        public TrajectoryOutcome SolvedOutcome => SolvedRun.Outcome;
    }
}
