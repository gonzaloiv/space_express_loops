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
    }
}
