using UnityEngine;

namespace DigitalLove.Input
{
    public class OneHandRotationGesture
    {
        private readonly float stepDistance;
        private bool tracking;
        private bool usingLeft;
        private Vector3 lastPoint;
        private float accumulated;
        private int appliedSteps;

        public OneHandRotationGesture(float stepDistance)
        {
            this.stepDistance = Mathf.Max(stepDistance, 0.0001f);
        }

        public int Update(in TwoHandGestureState state, Vector3 rightAxis)
        {
            if (!state.IsSingleHandPinching)
            {
                Reset();
                return 0;
            }

            bool left = state.IsLeftPinching;
            if (!tracking || left != usingLeft)
            {
                tracking = true;
                usingLeft = left;
                lastPoint = state.DragPoint;
                accumulated = 0f;
                appliedSteps = 0;
                return 0;
            }

            Vector3 point = state.DragPoint;
            accumulated += Vector3.Dot(point - lastPoint, rightAxis);
            lastPoint = point;

            int desired = Mathf.RoundToInt(accumulated / stepDistance);
            int delta = desired - appliedSteps;
            appliedSteps = desired;
            return delta;
        }

        public void Reset()
        {
            tracking = false;
            usingLeft = false;
            lastPoint = Vector3.zero;
            accumulated = 0f;
            appliedSteps = 0;
        }
    }
}
