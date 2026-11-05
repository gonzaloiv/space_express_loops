using DigitalLove.Input;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class CarouselTwoHandGesture
    {
        private const float ReArmProgress = 0.5f;

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
            float spread = ZoomProgress(state);
            float overview = OverviewProgress(state);
            if (spread < ReArmProgress)
                scaleInSent = false;

            bool enter = !scaleInSent && spread >= 1f;
            if (enter)
                scaleInSent = true;

            return new CarouselGestureOutcome(enter, spread, overview);
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

        private float ZoomProgress(in TwoHandGestureState state)
        {
            if (startDistance <= 0.0001f || settings.ScaleInMinRatio <= 1f)
                return 0f;

            float ratio = state.Distance / startDistance;
            float ratioProgress = (ratio - 1f) / (settings.ScaleInMinRatio - 1f);
            float farther = state.Distance - startDistance;
            float distanceProgress = farther / Mathf.Max(settings.ScaleInMinDistance, 0.0001f);
            return Mathf.Clamp01(Mathf.Min(ratioProgress, distanceProgress));
        }

        private float OverviewProgress(in TwoHandGestureState state)
        {
            if (startDistance <= 0.0001f || settings.OverviewMinRatio >= 1f)
                return 0f;

            float ratio = state.Distance / startDistance;
            float closer = startDistance - state.Distance;
            float ratioProgress = (1f - ratio) / (1f - settings.OverviewMinRatio);
            float distanceProgress = closer / Mathf.Max(settings.OverviewMinDistance, 0.0001f);
            return Mathf.Clamp01(Mathf.Min(ratioProgress, distanceProgress));
        }
    }
}
