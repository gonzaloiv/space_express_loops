namespace DigitalLove.Game.PuzzleSelection
{
    public readonly struct CarouselGestureSettings
    {
        public readonly float ScaleInMinRatio;
        public readonly float ScaleInMinDistance;
        public readonly float OverviewMinRatio;
        public readonly float OverviewMinDistance;

        public CarouselGestureSettings(
            float scaleInMinRatio,
            float scaleInMinDistance,
            float overviewMinRatio,
            float overviewMinDistance)
        {
            ScaleInMinRatio = scaleInMinRatio;
            ScaleInMinDistance = scaleInMinDistance;
            OverviewMinRatio = overviewMinRatio;
            OverviewMinDistance = overviewMinDistance;
        }

        public static CarouselGestureSettings Default => new(
            1.5f, 0.08f, 0.6f, 0.08f);
    }
}
