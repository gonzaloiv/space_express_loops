namespace DigitalLove.Game.Universe
{
    internal static class UniverseGhostFeedback
    {
        public static void Refresh(UniverseGhostView view, TwoHandGestureInput input)
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
