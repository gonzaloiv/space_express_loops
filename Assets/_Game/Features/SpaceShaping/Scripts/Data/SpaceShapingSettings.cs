using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    [CreateAssetMenu(fileName = "SpaceShapingSettings", menuName = "DigitalLove/Game/SpaceShaping Settings")]
    public class SpaceShapingSettings : ScriptableObject
    {
        [Header("Simulation")]
        public float stepSecs = 0.008333333f;
        public int maxSteps = 1200;

        [Header("Launch")]
        public float launchSpeed = 1f;

        [Header("Field")]
        public float sourceStrengthScale = 2f;
        public float separationScale = 1.2f;

        [Header("Time")]
        public float dilationScale = 1f;

        [Header("Sources")]
        public float captureRadius = 0.6f;

        [Header("Reachable scale")]
        public float minScaleFactor = 0.4f;
        public float maxScaleFactor = 2.5f;
    }
}
