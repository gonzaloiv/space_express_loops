using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    [CreateAssetMenu(fileName = "PuzzleSeed", menuName = "DigitalLove/Game/Puzzle Seed")]
    public class PuzzleSeed : ScriptableObject
    {
        public int seed = 1;
        public int planetCount = 9;

        [Tooltip("Planet arrangement. Auto keeps the seed-derived shape (Ring, Spiral, Cluster).")]
        public PuzzleFieldShape fieldShape = PuzzleFieldShape.Auto;

        [Header("Brief")]
        [Tooltip("The cube may be moved over the field. A fixed level places several cubes instead.")]
        public bool translatable;

        [Tooltip("How many zones the solution scales (1–3). A fixed level places that many cubes.")]
        [Min(1)]
        public int moves = 1;

        [Tooltip("The frame may be resized at runtime (1x to 2x).")]
        public bool resizable;
    }
}
