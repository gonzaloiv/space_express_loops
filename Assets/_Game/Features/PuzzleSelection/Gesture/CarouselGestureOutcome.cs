namespace DigitalLove.Game.PuzzleSelection
{
    public readonly struct CarouselGestureOutcome
    {
        public readonly int FocusDelta;
        public readonly bool Enter;
        public readonly float Speed;

        public CarouselGestureOutcome(int focusDelta, bool enter, float speed = 0f)
        {
            FocusDelta = focusDelta;
            Enter = enter;
            Speed = speed;
        }
    }
}
