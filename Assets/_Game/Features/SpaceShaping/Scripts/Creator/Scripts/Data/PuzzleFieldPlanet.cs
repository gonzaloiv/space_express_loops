using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    [System.Serializable]
    public struct PuzzleFieldPlanet
    {
        public Vector3 position;
        public float radius;

        public PuzzleFieldPlanet(Vector3 position, float radius)
        {
            this.position = position;
            this.radius = radius;
        }
    }
}
