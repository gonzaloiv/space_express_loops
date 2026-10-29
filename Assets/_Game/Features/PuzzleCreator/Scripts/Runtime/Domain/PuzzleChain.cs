namespace DigitalLove.Game.SpaceShaping.Creator
{
    internal readonly struct PuzzleChain
    {
        public readonly PuzzleZone[] Zones;
        public readonly PuzzleSolution Solution;
        public readonly float Margin;
        public readonly bool Arrived;

        public PuzzleChain(PuzzleZone[] zones, PuzzleSolution solution, float margin, bool arrived)
        {
            Zones = zones;
            Solution = solution;
            Margin = margin;
            Arrived = arrived;
        }

        public float Size => Zones[0].Width;
    }
}
