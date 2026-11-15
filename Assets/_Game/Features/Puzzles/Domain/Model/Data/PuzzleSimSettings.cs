namespace DigitalLove.Game.SpaceShaping
{
    public struct PuzzleSimSettings
    {
        public float RunDistance;
        public float RunReach;
        public float LaunchSpeed;
        public float StepSecs;
        public float NominalSpeed;
        public float SourceStrengthScale;
        public float SeparationScale;
        public float CaptureRadius;
        public float ShipClearance;
        public float ZoneFalloff;
        public float ZoneEffectGain;
        public float MinScaleFactor;
        public float MaxScaleFactor;

        public static PuzzleSimSettings From(SpaceShapingSettings settings) => new()
        {
            RunDistance = settings.runDistance,
            RunReach = settings.runReach,
            LaunchSpeed = settings.launchSpeedFactor * settings.nominalSpeed,
            StepSecs = settings.stepSecs,
            NominalSpeed = settings.nominalSpeed,
            SourceStrengthScale = settings.sourceStrengthScale,
            SeparationScale = settings.separationScale,
            CaptureRadius = settings.planetRadiusMax,
            ShipClearance = settings.shipClearance,
            ZoneFalloff = settings.zoneFalloff,
            ZoneEffectGain = settings.zoneEffectGain,
            MinScaleFactor = settings.minScaleFactor,
            MaxScaleFactor = settings.maxScaleFactor
        };
    }
}
