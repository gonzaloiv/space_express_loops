using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingSetup
    {
        public Vector3 Origin;
        public Vector3 Destination;
        public float CaptureRadius;
        public float StepSecs;
        public int MaxSteps;
        public float LaunchSpeed;
        public float SourceStrengthScale;
        public float SeparationScale;
        public float DilationScale;
        public float Tmin;
        public float Tmax;
        public float EntropyBudget;
        public SourceSpec[] Sources;
    }
}
