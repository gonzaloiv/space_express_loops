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
        public float cubeSizeMin;
        public float cubeSizeMax;
        public float cubeSize;
        public float captureRadius;
        public bool translatable;
        public int moves = 1;
        public bool resizable;
        public bool rotate;
        public float launchSpeed;
        public float tmin;
        public float tmax = 12f;
        public float entropyBudget = 1f;
        public PuzzleSource[] sources;
        public PuzzleZone[] fixedZones;

        public Vector3 Origin => origin;

        public Vector3 Destination => destination;

        public float Distance => Vector3.Distance(origin, destination);
    }
}
