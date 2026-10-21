namespace DigitalLove.Game.Universe
{
    public static class UniverseGhostFeedback
    {
        public static void Update(UniverseGhostView view, TwoHandGestureInput input)
        {
            bool leftPinching = input.IsLeftPinching;
            bool rightPinching = input.IsRightPinching;

            if (!leftPinching && !rightPinching)
            {
                view.Hide();
                return;
            }

            view.Show(
                input.LeftPinchPoint,
                input.RightPinchPoint,
                leftPinching,
                rightPinching,
                input.HandDistanceSpeed);
        }
    }
}
