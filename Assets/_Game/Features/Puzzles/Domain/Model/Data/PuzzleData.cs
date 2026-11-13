using DigitalLove.Casual.Levels;
using DigitalLove.Game.Spatial;
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
        [Tooltip("The cube only grows: a pinch cannot shrink it below its start (job 1; shrinking is "
            + "taught later).")]
        public bool growOnly;
        public float entropyBudget = 1f;
        [Tooltip("The tutorial cannot fail on entropy; the bar still shows the spend.")]
        public bool noFail;
        [Tooltip("The tutorial does not award or show stars.")]
        public bool noGrade;
        [Tooltip("The job plays no intro cue on start and shows no Edition card (job 1); the map "
            + "card still shows its contract.")]
        public bool noIntro;
        [Tooltip("Console line (Global table key) typed before the field appears: the tutorial's goal. "
            + "Empty plays none.")]
        public string goalLineKey;
        public PuzzleSource[] sources;
        [Tooltip("Extra ships. Empty is the single-ship puzzle (origin/destination); two or more makes "
            + "it a multi-ship level, solved when every ship arrives.")]
        public PuzzleShip[] ships;

        public string ClientKey => IntroKey + "_client";

        public string CargoKey => IntroKey + "_cargo";

        public string ManualKey => IntroKey + "_manual_entry";

        // The tech team ships the manual in releases: one per chapter, one patch per job, so job
        // 02_03 reads v2.0.3 of the chapter's release ("FLEET EXPANSION").
        public string ManualVersion => $"{int.Parse(GetChapterId(id))}.0.{AsIndex(id)}";

        public string ManualReleaseKey => $"chapter_{GetChapterId(id)}_manual_release";

        public Vector3 Origin => origin;

        public Vector3 Destination => destination;

        public float Distance => Vector3.Distance(origin, destination);

        // Every ship's endpoints, derived; single-ship data falls back to origin/destination.
        public PuzzleEndpoints Endpoints => new(ships, origin, destination);

        // The smallest scale a pinch may reach: the game's reach, or the start for a grow-only job.
        public float MinScaleFactor(float reachable) => growOnly ? 1f : reachable;

        public bool HasGoalLine => !string.IsNullOrEmpty(goalLineKey);

        public ZoneConditions ToZoneConditions() =>
            new(translatable, resizable, Endpoints.ToArray());

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
