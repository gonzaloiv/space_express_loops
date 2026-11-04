namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzlePostClearReport
    {
        public bool Tried;
        public bool Kept;
        public bool ClearBaked;
        public bool CurveBaked;
        public bool MixedBaked;
        public PuzzleSolutionKind MixedKind;
        public int MixedZones;
        public int BlockerIndex;
        public float BlockerRadius;
        public bool Moved;
        public bool Grown;
        public float ClearWidth;

        public override string ToString() =>
            $"probe tried {Tried}, kept {Kept}, clear {ClearBaked}, curve {CurveBaked}, " +
            $"mixed {MixedBaked}/{MixedKind}/{MixedZones}z, blocker {BlockerIndex} " +
            $"r {BlockerRadius:F3}, moved {Moved}, grown {Grown}, width {ClearWidth:F3}";
    }
}
