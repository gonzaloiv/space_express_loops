namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleSeedScanResult
    {
        public bool Found;
        public int Seed;
        public PuzzleBakeStatus Status;
        public PuzzleData Puzzle;
        public PuzzleZone[] Solution;
    }
}
