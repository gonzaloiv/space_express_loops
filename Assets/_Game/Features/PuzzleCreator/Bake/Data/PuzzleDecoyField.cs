namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The mutable view the decoy step edits: the planets, which of them are protected, the run and
    // the request's caps. A moved decoy keeps its radius and goes to a free spot of the generated shape.
    public sealed class PuzzleDecoyField
    {
        public PuzzleFieldPlanet[] Planets;
        public bool[] Protected;
        public PuzzleConstructRequest Request;
        public UnityEngine.Vector3 Origin;
        public UnityEngine.Vector3 Destination;
        // One entry per ship; empty falls back to the single Origin/Destination pair.
        public PuzzleConstructedShip[] Ships;
    }
}
