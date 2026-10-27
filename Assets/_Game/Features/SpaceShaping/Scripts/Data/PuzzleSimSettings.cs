namespace DigitalLove.Game.SpaceShaping
{
    public struct PuzzleSimSettings
    {
        public float RunDistance;
        public float StepSecs;
        public float NominalSpeed;
        public float SourceStrengthScale;
        public float SeparationScale;
        public float LightSpeed;
        public float CaptureRadius;
        public float ZoneFalloff;
        public float MinScaleFactor;
        public float MaxScaleFactor;

        public static PuzzleSimSettings From(SpaceShapingSettings settings) => new()
        {
            RunDistance = settings.runDistance,
            StepSecs = settings.stepSecs,
            NominalSpeed = settings.nominalSpeed,
            SourceStrengthScale = settings.sourceStrengthScale,
            SeparationScale = settings.separationScale,
            LightSpeed = settings.lightSpeed,
            CaptureRadius = settings.planetRadiusMin,
            ZoneFalloff = settings.zoneFalloff,
            MinScaleFactor = settings.minScaleFactor,
            MaxScaleFactor = settings.maxScaleFactor
        };
    }
}
