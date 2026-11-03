using UnityEngine;

namespace DigitalLove.Input
{
    public class TwoHandYawGesture
    {
        private readonly float minSeparation;
        private readonly float maxVerticalRatio;
        private bool tracked;
        private float lastYaw;
        private float accumulatedYaw;

        public TwoHandYawGesture(float minSeparation, float maxVerticalRatio)
        {
            this.minSeparation = Mathf.Max(minSeparation, 0f);
            this.maxVerticalRatio = Mathf.Max(maxVerticalRatio, 0f);
        }

        public float AccumulatedYaw => accumulatedYaw;

        public float Update(in TwoHandGestureState state)
        {
            if (!state.IsActive)
            {
                Reset();
                return 0f;
            }

            float yaw = Yaw(state);
            if (!tracked)
            {
                tracked = true;
                lastYaw = yaw;
                accumulatedYaw = 0f;
                return 0f;
            }

            float delta = 0f;
            if (IsValid(state))
            {
                delta = Mathf.DeltaAngle(lastYaw, yaw);
                accumulatedYaw += delta;
            }

            lastYaw = yaw;
            return delta;
        }

        public void Reset()
        {
            tracked = false;
            lastYaw = 0f;
            accumulatedYaw = 0f;
        }

        private bool IsValid(in TwoHandGestureState state)
        {
            Vector3 line = state.Right.Point - state.Left.Point;
            float horizontal = Mathf.Sqrt(line.x * line.x + line.z * line.z);
            if (horizontal < minSeparation)
                return false;

            return Mathf.Abs(line.y) <= horizontal * maxVerticalRatio;
        }

        private static float Yaw(in TwoHandGestureState state)
        {
            Vector3 line = state.Right.Point - state.Left.Point;
            return Mathf.Atan2(line.x, line.z) * Mathf.Rad2Deg;
        }
    }
}
