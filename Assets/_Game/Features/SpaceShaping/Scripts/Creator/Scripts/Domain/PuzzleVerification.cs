namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleVerification
    {
        public TrajectoryOutcome StartOutcome;
        public TrajectoryOutcome SolvedOutcome;
        public float ShipTime;
        public float EntropySpent;
        public bool Solved;

        public bool StartFails => StartOutcome != TrajectoryOutcome.Arrive;
    }
}
