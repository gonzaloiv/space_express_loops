using DigitalLove.Casual.Levels;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    [CreateAssetMenu(fileName = "PuzzleData", menuName = "DigitalLove/Game/PuzzleData")]
    public class PuzzleData : LevelData
    {
        [Header("Space Shaping")]
        public Vector3 origin = new(-4f, 0f, 0f);
        public Vector3 destination = new(4f, 0f, 0f);
        public float captureRadius = 0.6f;
        public float tmin;
        public float tmax = 12f;
        public float entropyBudget = 1f;
        public float entropyPar = 0.5f;
        public PuzzleSource[] sources;
    }
}
