using DigitalLove.Input;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class CarouselTwoHandGesture
    {
        private readonly CarouselGestureSettings settings;
        private bool isActive;
        private bool scaleInSent;
        private float startDistance;

        public CarouselTwoHandGesture(CarouselGestureSettings settings)
        {
            this.settings = settings;
        }

        public CarouselGestureOutcome Update(in TwoHandGestureState state)
        {
            if (!state.IsActive)
                return default;

            EnsureActive(state);
            if (scaleInSent || !IsScaleIn(state))
                return default;

            scaleInSent = true;
            return new CarouselGestureOutcome(0, true);
        }

        public void Reset()
        {
            isActive = false;
            scaleInSent = false;
            startDistance = 0f;
        }

        private void EnsureActive(in TwoHandGestureState state)
        {
            if (isActive)
                return;

            isActive = true;
            startDistance = state.Distance;
        }

        private bool IsScaleIn(in TwoHandGestureState state)
        {
            if (startDistance <= 0.0001f)
                return false;

            float farther = state.Distance - startDistance;
            float ratio = state.Distance / startDistance;
            return farther >= settings.ScaleInMinDistance
                && ratio >= settings.ScaleInMinRatio;
        }
    }
}
