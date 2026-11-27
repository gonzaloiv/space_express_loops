namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The mutable view the decoy step edits: the planets, which of them are protected, the run, the
    // request's caps, and the field radius the replacement positions are sampled inside.
    public sealed class PuzzleDecoyField
    {
        public PuzzleFieldPlanet[] Planets;
        public bool[] Protected;
        public PuzzleConstructRequest Request;
        public UnityEngine.Vector3 Origin;
        public UnityEngine.Vector3 Destination;
        public float Radius;
    }
}
