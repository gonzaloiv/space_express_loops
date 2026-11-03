using DigitalLove.Input;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class CarouselTwoHandGesture
    {
        private enum Mode
        {
            None,
            Rotate,
            Amplify
        }

        private readonly CarouselGestureSettings settings;
        private Mode mode;
        private bool isActive;
        private bool amplifySent;
        private float startDistance;
        private float lastYaw;
        private float accumulatedYaw;
        private int appliedRotationSteps;

        public CarouselTwoHandGesture(CarouselGestureSettings settings)
        {
            this.settings = settings;
        }

        public CarouselGestureOutcome Update(in TwoHandGestureState state)
        {
            if (!state.IsActive)
                return default;

            EnsureActive(state);
            AccumulateYaw(state);
            if (mode == Mode.None)
                mode = Classify(state);

            if (mode == Mode.Rotate)
                return new CarouselGestureOutcome(ChangeRotationStep(), false);
            if (mode == Mode.Amplify)
                return new CarouselGestureOutcome(0, TryAmplify(state));

            return default;
        }

        public void Reset()
        {
            mode = Mode.None;
            isActive = false;
            amplifySent = false;
            startDistance = 0f;
            lastYaw = 0f;
            accumulatedYaw = 0f;
            appliedRotationSteps = 0;
        }

        private void EnsureActive(in TwoHandGestureState state)
        {
            if (isActive)
                return;

            isActive = true;
            startDistance = state.Distance;
            lastYaw = Yaw(state);
            accumulatedYaw = 0f;
            appliedRotationSteps = 0;
        }

        private void AccumulateYaw(in TwoHandGestureState state)
        {
            float yaw = Yaw(state);
            if (RotationIsValid(state))
                accumulatedYaw += Mathf.DeltaAngle(lastYaw, yaw);
            lastYaw = yaw;
        }

        private Mode Classify(in TwoHandGestureState state)
        {
            float rotate = Mathf.Abs(accumulatedYaw)
                / Mathf.Max(settings.RotationThresholdDegrees, 0.001f);
            float amplify = AmplifyProgress(state);
            if (rotate < 1f && amplify < 1f)
                return Mode.None;

            return amplify >= rotate ? Mode.Amplify : Mode.Rotate;
        }

        private int ChangeRotationStep()
        {
            float stepAngle = Mathf.Max(settings.StepAngleDegrees, 0.001f);
            int desired = Mathf.RoundToInt(accumulatedYaw / stepAngle);
            if (desired == appliedRotationSteps)
                return 0;

            float walked = Mathf.Abs(accumulatedYaw - appliedRotationSteps * stepAngle);
            if (walked < settings.RotationThresholdDegrees)
                return 0;

            int delta = desired - appliedRotationSteps;
            appliedRotationSteps = desired;
            return delta;
        }

        private bool TryAmplify(in TwoHandGestureState state)
        {
            if (amplifySent || AmplifyProgress(state) < 1f)
                return false;

            amplifySent = true;
            return true;
        }

        private float AmplifyProgress(in TwoHandGestureState state)
        {
            float delta = state.Distance - startDistance;
            float ratio = startDistance > 0.0001f ? state.Distance / startDistance : 1f;
            float minDistance = Mathf.Max(settings.AmplifyMinDistance, 0.0001f);
            if (delta < minDistance || ratio < settings.AmplifyMinRatio)
                return 0f;

            return Mathf.Max(delta / minDistance, ratio / settings.AmplifyMinRatio);
        }

        private bool RotationIsValid(in TwoHandGestureState state)
        {
            Vector3 line = state.Right.Point - state.Left.Point;
            float horizontal = Mathf.Sqrt(line.x * line.x + line.z * line.z);
            if (horizontal < settings.RotationMinSeparation)
                return false;

            return Mathf.Abs(line.y) <= horizontal * settings.RotationMaxVerticalRatio;
        }

        private static float Yaw(in TwoHandGestureState state)
        {
            Vector3 line = state.Right.Point - state.Left.Point;
            return Mathf.Atan2(line.x, line.z) * Mathf.Rad2Deg;
        }
    }
}
