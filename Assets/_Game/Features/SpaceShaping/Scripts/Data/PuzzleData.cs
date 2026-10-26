using DigitalLove.Casual.Levels;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    [CreateAssetMenu(fileName = "PuzzleData", menuName = "DigitalLove/Game/PuzzleData")]
    public class PuzzleData : LevelData
    {
        [Header("Space Shaping")]
        public float distance = 8f;
        public float tmin;
        public float tmax = 12f;
        public float entropyBudget = 1f;
        public PuzzleSource[] sources;
        public PuzzleZone[] solution;

        public Vector3 Origin => new(distance * -0.5f, 0f, 0f);

        public Vector3 Destination => new(distance * 0.5f, 0f, 0f);

        public Vector3 ZoneSize => new(distance, distance, distance);
    }
}
