using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The answer a job was baked with. The shipped check replays these zones instead of re-solving, so
    // it no longer depends on today's search space. Stored outside Resources, so it never ships.
    public class PuzzleAnswerAsset : ScriptableObject
    {
        public const string Folder = "Assets/_Game/Features/PuzzleCreator/Answers";

        public string id;
        public PuzzleSolutionKind kind;
        public PuzzleZone[] zones = System.Array.Empty<PuzzleZone>();

        [Tooltip("The contract seed that baked the job; a report value, not the answer.")]
        public int seed;

        [Header("Bake report")]
        [Tooltip("Share of the scale range around the answer that still lands every ship.")]
        public float window;

        [Tooltip("The answer's |scale - 1|.")]
        public float scaleDelta;

        [Tooltip("Distance from the start frame to the answer on a movable job; 0 on a fixed job.")]
        public float startDistance;

        public static string PathFor(string id) => $"{Folder}/{id}.asset";
    }
}
