using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    [CreateAssetMenu(fileName = "PuzzleSeed", menuName = "DigitalLove/Game/Puzzle Seed")]
    public class PuzzleSeed : ScriptableObject
    {
        public int seed = 1;
        [HideInInspector] public float distance = 1f;
        public int planetCount = 9;

        [Header("Brief")]
        [Tooltip("The cube may be moved over the field. Fixed levels bake the cube where the solution sits.")]
        public bool translatable;

        [Tooltip("How many zones the solution scales. The creator ships one zone; multi-zone bands wait for the multi-zone search.")]
        [Min(1)]
        public int moves = 1;

        [Tooltip("The frame may be resized at runtime (1x to 2x).")]
        public bool resizable;

        [Tooltip("The spin gesture is available. Reserved for later puzzles.")]
        public bool rotate;

        [Tooltip("Window half-width as a fraction of the crossing time. 0.5 is a +/-50% band around run / nominalSpeed.")]
        [Range(0f, 1f)]
        public float windowTightness = 0.5f;
    }
}
