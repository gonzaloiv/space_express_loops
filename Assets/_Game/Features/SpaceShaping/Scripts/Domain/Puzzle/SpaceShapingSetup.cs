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
        public LaunchModel LaunchModel;
        public float LaunchSpeed;
        public float ThrustAcceleration;
        public float SourceStrengthScale;
        public float SeparationScale;
        public float DilationScale;
        public float Tmin;
        public float Tmax;
        public float EntropyBudget;
        public float EntropyPar;
        public SourceSpec[] Sources;
    }
}
