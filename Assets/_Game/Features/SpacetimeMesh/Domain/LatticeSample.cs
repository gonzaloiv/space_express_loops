namespace DigitalLove.Game.SpaceShaping.Grid
{
    // Where one lattice mesh vertex sits: which axis's line, the line's (I, J) in the grid, its ring
    // side, and how far along the line (in grid steps).
    public readonly struct LatticeSample
    {
        public readonly int Axis;
        public readonly int I;
        public readonly int J;
        public readonly int Side;
        public readonly float NodeIndex;

        public LatticeSample(int axis, int i, int j, int side, float nodeIndex)
        {
            Axis = axis;
            I = i;
            J = j;
            Side = side;
            NodeIndex = nodeIndex;
        }
    }
}
