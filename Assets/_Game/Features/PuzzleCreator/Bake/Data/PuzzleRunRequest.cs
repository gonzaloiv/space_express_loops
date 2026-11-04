namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleRunRequest
    {
        public PuzzleFieldPlanet[] Planets;
        public float TargetRun;
        public float EndpointClearance;
        public float SunRadius;
        public bool RequirePlanetBlocker;
        public bool AllowSunOnChord;
    }
}
