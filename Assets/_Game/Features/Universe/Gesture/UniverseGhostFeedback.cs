using DigitalLove.Input;

namespace DigitalLove.Game.Universe
{
    internal static class UniverseGhostFeedback
    {
        public static void Refresh(UniverseGhostView view, TwoHandGestureState state)
        {
            bool leftPinching = state.IsLeftPinching;
            bool rightPinching = state.IsRightPinching;

            if (!leftPinching && !rightPinching)
            {
                view.Hide();
                return;
            }

            view.Show(
                state.Left.Point,
                state.Right.Point,
                leftPinching,
                rightPinching,
                state.DistanceSpeed);
        }
    }
}
