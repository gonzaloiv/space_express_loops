using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    [System.Serializable]
    public struct PuzzleZone
    {
        public Vector3 centre;
        public Vector3 size;
        public float scale;
        public float falloff;

        public SpaceZone ToSpaceZone() => new(centre, size * 0.5f, scale, falloff);
    }
}
