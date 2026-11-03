using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    [System.Serializable]
    public struct PuzzleFieldPlanet
    {
        public Vector3 position;
        public float radius;
        public string name;

        public PuzzleFieldPlanet(Vector3 position, float radius, string name = "")
        {
            this.position = position;
            this.radius = radius;
            this.name = name;
        }
    }
}
