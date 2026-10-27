using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    [CreateAssetMenu(fileName = "PuzzleSeed", menuName = "DigitalLove/Game/Puzzle Seed")]
    public class PuzzleSeed : ScriptableObject
    {
        public int seed = 1;
        [HideInInspector] public float distance = 1f;
        public int planetCount = 9;

        [Tooltip("Minimum clearance between the run's endpoint planets and every other planet, in metres. Wider = a clearer corridor and a denser field is allowed.")]
        public float corridorClearance = 0.05f;

        [Tooltip("Smallest cube the creator may bake for this level, in metres. Smaller is a sharper, more local edit.")]
        public float minCubeSize = 0.15f;

        [Tooltip("Largest cube the creator may bake for this level, in metres. Bigger is a blunter, more forgiving lever.")]
        public float maxCubeSize = 0.3f;

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
