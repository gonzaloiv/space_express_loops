using DigitalLove.Casual.Levels;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;
using UnityEngine.Serialization;

namespace DigitalLove.Game.SpaceShaping
{
    [CreateAssetMenu(fileName = "PuzzleData", menuName = "DigitalLove/Game/PuzzleData")]
    public class PuzzleData : LevelData
    {
        [Header("Space Shaping")]
        public Vector3 origin;
        public Vector3 destination;
        [Tooltip("Run midpoint; the start frame a movable level ships.")]
        public Vector3 zoneCentre;
        public float cubeSizeMin;
        public float cubeSizeMax;
        [Tooltip("The cube frames the level places: position + size. A fixed level ships the solved "
            + "zones; a movable level ships one start frame.")]
        public PuzzleFrame[] frames;
        public float captureRadius;
        public bool translatable;
        [Tooltip("Zones the solution scales — the bake's zone count, not the cubes placed. A movable "
            + "level ships one start frame and the player moves it between zones.")]
        [FormerlySerializedAs("moves")]
        public int zones = 1;
        [Tooltip("Baked route clusters: distinct zone sets that arrive within budget.")]
        public int routes = 1;
        public bool resizable;
        public float entropyBudget = 1f;
        [Tooltip("The tutorial cannot fail on entropy; the bar still shows the spend.")]
        public bool noFail;
        [Tooltip("The tutorial does not award or show stars.")]
        public bool noGrade;
        [Tooltip("The job plays no intro cue on start and shows no Edition card (job 1); the map "
            + "card still shows its contract.")]
        public bool noIntro;
        public PuzzleSource[] sources;
        [Tooltip("Extra ships. Empty is the single-ship puzzle (origin/destination); two or more makes "
            + "it a multi-ship level, solved when every ship arrives.")]
        public PuzzleShip[] ships;

        public string ClientKey => IntroKey + "_client";

        public string CargoKey => IntroKey + "_cargo";

        public string ManualKey => IntroKey + "_manual_entry";

        // The tech team rewrites the manual after each delivery; job N reads revision N.
        public int ManualRevision => AsIndex(id);

        public Vector3 Origin => origin;

        public Vector3 Destination => destination;

        public float Distance => Vector3.Distance(origin, destination);

        public ZoneConditions ToZoneConditions() =>
            new(translatable, resizable, origin, destination);

        public float CaptureRadiusAtDestination() => CaptureRadiusAt(destination);

        public float CaptureRadiusAt(Vector3 point)
        {
            if (sources == null)
                return 0f;

            for (int i = 0; i < sources.Length; i++)
            {
                if ((sources[i].home - point).sqrMagnitude < 0.000001f)
                    return sources[i].scale * 0.5f;
            }

            return 0f;
        }
    }
}
