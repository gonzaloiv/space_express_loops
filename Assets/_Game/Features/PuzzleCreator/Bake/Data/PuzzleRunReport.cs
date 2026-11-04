namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleRunReport
    {
        public int Pairs;
        public int LengthRejected;
        public int ClearanceRejected;
        public int NoPlanetBlocker;
        public int SunOnChord;
        public int NoBlocker;
        public int Accepted;

        public override string ToString() =>
            $"pairs {Pairs}, length {LengthRejected}, clearance {ClearanceRejected}, " +
            $"no planet {NoPlanetBlocker}, sun chord {SunOnChord}, no blocker {NoBlocker}, " +
            $"accepted {Accepted}";
    }
}
