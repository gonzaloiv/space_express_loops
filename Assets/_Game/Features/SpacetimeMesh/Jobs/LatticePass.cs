using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    // One lattice render's inputs: where the lattice sits in the field, how big it is, and its look.
    public struct LatticePass
    {
        public Matrix4x4 LocalToParent;
        public Matrix4x4 ParentToLocal;
        public float Half;
        public float Radius;
        public int SmoothPasses;
        public LatticeStrainLook Look;
    }
}
