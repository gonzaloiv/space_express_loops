namespace DigitalLove.Game.PuzzleSelection
{
    public readonly struct CarouselGestureSettings
    {
        public readonly float ScaleInMinRatio;
        public readonly float ScaleInMinDistance;
        public readonly float MinSwipeDistance;
        public readonly float MinSwipeSpeed;
        public readonly float SwipeHorizontalDominance;

        public CarouselGestureSettings(
            float scaleInMinRatio,
            float scaleInMinDistance,
            float minSwipeDistance,
            float minSwipeSpeed,
            float swipeHorizontalDominance)
        {
            ScaleInMinRatio = scaleInMinRatio;
            ScaleInMinDistance = scaleInMinDistance;
            MinSwipeDistance = minSwipeDistance;
            MinSwipeSpeed = minSwipeSpeed;
            SwipeHorizontalDominance = swipeHorizontalDominance;
        }

        public static CarouselGestureSettings Default => new(
            1.15f, 0.06f, 0.03f, 0.25f, 1.5f);
    }
}
