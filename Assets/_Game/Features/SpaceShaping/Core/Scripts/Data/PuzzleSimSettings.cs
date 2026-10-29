namespace DigitalLove.Game.SpaceShaping
{
    public struct PuzzleSimSettings
    {
        public float RunDistance;
        public float LaunchSpeed;
        public float StepSecs;
        public float NominalSpeed;
        public float SourceStrengthScale;
        public float SeparationScale;
        public float CaptureRadius;
        public float ZoneFalloff;
        public float MinScaleFactor;
        public float MaxScaleFactor;

        public static PuzzleSimSettings From(SpaceShapingSettings settings) => new()
        {
            RunDistance = settings.runDistance,
            LaunchSpeed = settings.launchSpeed,
            StepSecs = settings.stepSecs,
            NominalSpeed = settings.nominalSpeed,
            SourceStrengthScale = settings.sourceStrengthScale,
            SeparationScale = settings.separationScale,
            CaptureRadius = settings.planetRadiusMax,
            ZoneFalloff = settings.zoneFalloff,
            MinScaleFactor = settings.minScaleFactor,
            MaxScaleFactor = settings.maxScaleFactor
        };
    }
}
