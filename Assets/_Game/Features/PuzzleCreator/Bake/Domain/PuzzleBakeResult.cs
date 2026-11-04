namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleBakeResult
    {
        public PuzzleData Puzzle;
        public PuzzleBakeStatus Status;
        public PuzzleZone[] Solution;
        public PuzzleSolutionKind Kind;
        public PuzzleRunReport RunReport;
        public PuzzlePostClearReport PostClear;

        public bool Baked => Puzzle != null;
    }
}
