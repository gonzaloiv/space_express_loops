namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The mutable view the decoy step edits: the planets, which of them are protected, the run and
    // the request's caps. A moved decoy keeps its radius, so a Ring field's decoys stay on its rings.
    public sealed class PuzzleDecoyField
    {
        public PuzzleFieldPlanet[] Planets;
        public bool[] Protected;
        public PuzzleConstructRequest Request;
        public UnityEngine.Vector3 Origin;
        public UnityEngine.Vector3 Destination;
    }
}
