namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleJobContract
    {
        public string Id;
        public bool Pending;
        public int Seed;
        public int Attempts;
        public int PlanetCount;
        public PuzzleFieldShape FieldShape;
        public bool HasSun;
        public bool SunOnChord;
        public PuzzleSunRule SunRule;
        public int MinBlockers;
        public PuzzleMechanism Mechanism;
        public int Zones;
        public int MinRoutes;
        public bool Translatable;
        public PuzzleStartRule StartRule;
        public bool NoFail;
        public bool NoGrade;
    }
}
