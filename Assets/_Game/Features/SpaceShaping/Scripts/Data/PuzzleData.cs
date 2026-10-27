using DigitalLove.Casual.Levels;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    [CreateAssetMenu(fileName = "PuzzleData", menuName = "DigitalLove/Game/PuzzleData")]
    public class PuzzleData : LevelData
    {
        [Header("Space Shaping")]
        public Vector3 origin;
        public Vector3 destination;
        public Vector3 zoneCentre;
        public float launchSpeed;
        public float tmin;
        public float tmax = 12f;
        public float entropyBudget = 1f;
        public PuzzleSource[] sources;
        public PuzzleZone[] solution;

        public Vector3 Origin => origin;

        public Vector3 Destination => destination;

        public float Distance => Vector3.Distance(origin, destination);
    }
}
