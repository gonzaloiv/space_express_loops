using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    [CreateAssetMenu(fileName = "PuzzleSeed", menuName = "DigitalLove/Game/Puzzle Seed")]
    public class PuzzleSeed : ScriptableObject
    {
        public int seed = 1;
        [HideInInspector] public float distance = 1f;
        public int planetCount = 9;

        [Tooltip("Planet arrangement. Auto keeps the seed-derived shape (Ring, Spiral, Cluster).")]
        public PuzzleFieldShape fieldShape = PuzzleFieldShape.Auto;

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

        [Tooltip("Launch speed as a fraction of nominal. Below 1 the bare start runs out of step budget and misses; 1 arrives on the nominal crossing.")]
        [Range(0.5f, 1.5f)]
        public float startMargin = 0.9f;

        [Tooltip("The time tutorial: the fixed cube sits on a planet off the trajectory, so the scale moves the clock without bending the path. The window is baked around the found clock solution.")]
        public bool offPath;

        [Tooltip("Window half-width as a fraction of the nominal proper crossing. 0.5 is a +/-50% band around the ship-clock crossing at nominal speed; a clock level needs a much tighter band.")]
        [Range(0f, 1f)]
        public float windowTightness = 0.5f;
    }
}
