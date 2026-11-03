using UnityEngine;

namespace DigitalLove.Input
{
    public class ScaleOutGesture
    {
        private readonly float minDistance;
        private readonly float maxRatio;
        private bool tracking;
        private bool fired;
        private float startDistance;

        public ScaleOutGesture(float minDistance, float maxRatio)
        {
            this.minDistance = Mathf.Max(minDistance, 0f);
            this.maxRatio = Mathf.Clamp01(maxRatio);
        }

        public bool Update(in TwoHandGestureState state)
        {
            if (!state.IsActive)
            {
                Reset();
                return false;
            }

            if (!tracking)
            {
                tracking = true;
                fired = false;
                startDistance = state.Distance;
            }

            if (fired || startDistance <= 0.0001f)
                return false;

            float closer = startDistance - state.Distance;
            float ratio = state.Distance / startDistance;
            if (closer < minDistance || ratio > maxRatio)
                return false;

            fired = true;
            return true;
        }

        public void Reset()
        {
            tracking = false;
            fired = false;
            startDistance = 0f;
        }
    }
}
