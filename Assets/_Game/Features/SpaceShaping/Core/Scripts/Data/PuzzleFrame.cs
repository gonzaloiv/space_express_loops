using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    [System.Serializable]
    public struct PuzzleFrame
    {
        public Vector3 centre;
        public float size;

        public Vector3 BoxSize => Vector3.one * size;
    }
}
