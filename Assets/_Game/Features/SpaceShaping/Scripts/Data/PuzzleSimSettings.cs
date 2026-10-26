namespace DigitalLove.Game.SpaceShaping
{
    public struct PuzzleSimSettings
    {
        public float StepSecs;
        public int MaxSteps;
        public float LaunchSpeed;
        public float SourceStrengthScale;
        public float SeparationScale;
        public float DilationScale;
        public float CaptureRadius;
        public float MinScaleFactor;
        public float MaxScaleFactor;

        public static PuzzleSimSettings From(SpaceShapingSettings settings) => new()
        {
            StepSecs = settings.stepSecs,
            MaxSteps = settings.maxSteps,
            LaunchSpeed = settings.launchSpeed,
            SourceStrengthScale = settings.sourceStrengthScale,
            SeparationScale = settings.separationScale,
            DilationScale = settings.dilationScale,
            CaptureRadius = settings.captureRadius,
            MinScaleFactor = settings.minScaleFactor,
            MaxScaleFactor = settings.maxScaleFactor
        };
    }
}
