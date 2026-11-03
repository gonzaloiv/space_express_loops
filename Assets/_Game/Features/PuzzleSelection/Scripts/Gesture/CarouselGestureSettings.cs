namespace DigitalLove.Game.PuzzleSelection
{
    public readonly struct CarouselGestureSettings
    {
        public readonly float StepAngleDegrees;
        public readonly float RotationHysteresisDegrees;
        public readonly float RotationMinSeparation;
        public readonly float RotationMaxVerticalRatio;
        public readonly float AmplifyMinRatio;
        public readonly float AmplifyMinDistance;
        public readonly float MinSwipeDistance;
        public readonly float MinSwipeSpeed;
        public readonly float SwipeHorizontalDominance;

        public CarouselGestureSettings(
            float stepAngleDegrees,
            float rotationHysteresisDegrees,
            float rotationMinSeparation,
            float rotationMaxVerticalRatio,
            float amplifyMinRatio,
            float amplifyMinDistance,
            float minSwipeDistance,
            float minSwipeSpeed,
            float swipeHorizontalDominance)
        {
            StepAngleDegrees = stepAngleDegrees;
            RotationHysteresisDegrees = rotationHysteresisDegrees;
            RotationMinSeparation = rotationMinSeparation;
            RotationMaxVerticalRatio = rotationMaxVerticalRatio;
            AmplifyMinRatio = amplifyMinRatio;
            AmplifyMinDistance = amplifyMinDistance;
            MinSwipeDistance = minSwipeDistance;
            MinSwipeSpeed = minSwipeSpeed;
            SwipeHorizontalDominance = swipeHorizontalDominance;
        }

        public float RotationThresholdDegrees =>
            StepAngleDegrees * 0.5f + RotationHysteresisDegrees;

        public static CarouselGestureSettings Default => new(
            35f, 10f, 0.12f, 1f, 1.2f, 0.08f, 0.03f, 0.25f, 1.5f);
    }
}
