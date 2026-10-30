using UnityEngine;
using UnityEngine.Serialization;

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

        [Tooltip("Zones the solution scales (1–3) — the bake's zone count, not the cubes placed. A fixed "
            + "level places that many cubes; a movable level ships one start frame the player moves.")]
        [Min(1)]
        [FormerlySerializedAs("moves")]
        public int zones = 1;

        [Tooltip("Ships the level must land. 1 is a single-ship puzzle; 2 or more bakes a multi-ship "
            + "level whose runs relate as the relation field says.")]
        [Min(1)]
        public int ships = 1;

        [Tooltip("How multi-ship runs relate: FanOut shares one origin with different destinations; "
            + "Converge shares one destination with different origins; Separate shares no planet.")]
        public PuzzleShipRelation relation = PuzzleShipRelation.Separate;

        [Tooltip("The frame may be resized at runtime (1x to 2x).")]
        public bool resizable;
    }
}
