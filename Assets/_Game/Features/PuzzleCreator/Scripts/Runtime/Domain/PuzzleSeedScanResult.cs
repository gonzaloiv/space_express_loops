namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleSeedScanResult
    {
        public bool Found;
        public int Seed;
        public int Routes;
        public PuzzleBakeStatus Status;
        public PuzzleData Puzzle;
        public PuzzleZone[] Solution;
        public int Attempts;
        public int Baked;
        public bool HasFailure;
        public PuzzleBakeStatus FirstFailure;
        public int[] Failures;
    }
}
