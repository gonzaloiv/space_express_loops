namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleBakeResult
    {
        public PuzzleData Puzzle;
        public PuzzleBakeStatus Status;
        public PuzzleZone[] Solution;
        public PuzzleSolutionKind Kind;
        public PuzzleRunReport RunReport;
        // The run of arriving scales around the answer, as a share of the range (the verdict measures
        // it). A zero window is a knife edge.
        public float Window;
        // The reason a bake failed: the validator's lines on ContractFailed, the build step on NoRun;
        // null otherwise.
        public System.Collections.Generic.List<string> Problems;

        public bool Baked => Puzzle != null;
    }
}
