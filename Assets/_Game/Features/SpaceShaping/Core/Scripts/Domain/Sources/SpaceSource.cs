using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public struct SpaceSource
    {
        public Vector3 Position;
        public float Radius;
        public float Strength;

        public SpaceSource(Vector3 position, float radius, float strength)
        {
            Position = position;
            Radius = radius;
            Strength = strength;
        }
    }
}
