using System.Collections.Generic;

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
        public PuzzleSolutionKind Kind;
        public PuzzleRunReport RunReport;
        // The constructor's verdict measures these; 0/false means the scan still has to.
        public float Window;
        public bool ContractChecked;
        public List<string> Problems;
        public int Attempts;
        public int Baked;
        public bool HasFailure;
        public PuzzleBakeStatus FirstFailure;
        public int[] Failures;
    }
}
