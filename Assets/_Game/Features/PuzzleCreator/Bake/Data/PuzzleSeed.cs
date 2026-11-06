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

        [Header("Sun")]
        [Tooltip("Bake a sun at the field's centre, sized by the settings' sunRadius.")]
        public bool hasSun;

        [Tooltip("What the solution must do with the sun: Any, InZone, CentredGrown, or OutOfZone.")]
        public PuzzleSunRule sunRule = PuzzleSunRule.Any;

        [Tooltip("Whether the run chord may pass through the sun.")]
        public bool sunOnChord;

        [Header("Brief")]
        [Tooltip("The cube may be moved over the field. A fixed level places several cubes instead.")]
        public bool translatable;

        [Tooltip("Where a movable level starts: the run midpoint, or the field centre (the sun).")]
        public PuzzleStartRule startRule = PuzzleStartRule.RunMidpoint;

        [Tooltip("Zones the solution scales (1–3) — the bake's zone count, not the cubes placed. A fixed "
            + "level places that many cubes; a movable level ships one start frame the player moves.")]
        [Min(1)]
        [FormerlySerializedAs("moves")]
        public int zones = 1;

        [Tooltip("The role the bake must produce: Any, a grown well (Curve), a shrunk obstacle "
            + "(Clear), or both (Mixed, needs at least two zones).")]
        public PuzzleMechanism mechanism = PuzzleMechanism.Any;

        [Tooltip("Minimum distinct arriving routes the bake must offer. 0 = auto: 2 for a movable "
            + "single-zone level, otherwise 1. Set 2 for the capstone multi-zone jobs.")]
        [Min(0)]
        public int minRoutes;

        [Tooltip("Ships the level must land. 1 is a single-ship puzzle; 2 or more bakes a multi-ship "
            + "level whose runs relate as the relation field says.")]
        [Min(1)]
        public int ships = 1;

        [Tooltip("How multi-ship runs relate: FanOut shares one origin with different destinations; "
            + "Converge shares one destination with different origins; Separate shares no planet.")]
        public PuzzleShipRelation relation = PuzzleShipRelation.Separate;

        [Tooltip("The frame may be resized at runtime (1x to 2x).")]
        public bool resizable;

        [Header("Tutorial")]
        [Tooltip("The tutorial cannot fail on entropy; the bar still shows the spend.")]
        public bool noFail;

        [Tooltip("The tutorial does not award or show stars.")]
        public bool noGrade;

        [Tooltip("The job plays no intro cue on start.")]
        public bool noIntro;

        public float EffectiveSunRadius(SpaceShapingSettings settings) =>
            hasSun ? settings.sunRadius : 0f;
    }
}
