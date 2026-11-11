using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    // The cube's latest refused move: where the hand wanted the frame and which endpoint blocked it.
    public struct CubeMoveRefusal
    {
        public bool Blocked { get; private set; }

        public int Endpoint { get; private set; }

        public Vector3 Centre { get; private set; }

        public Vector3 Size { get; private set; }

        public void Set(int endpoint, Vector3 centre, Vector3 size)
        {
            Blocked = true;
            Endpoint = endpoint;
            Centre = centre;
            Size = size;
        }

        public void Clear() => Blocked = false;
    }
}
