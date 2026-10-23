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
        public LaunchModel launchModel = LaunchModel.Coast;
        public float launchSpeed = 1f;
        public float thrustAcceleration = 1f;

        [Header("Field")]
        public float sourceStrengthScale = 2f;
        public float separationScale = 1.2f;

        [Header("Time")]
        public float dilationScale = 1f;
    }
}
