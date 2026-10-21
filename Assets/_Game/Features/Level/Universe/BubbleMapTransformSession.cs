using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class BubbleMapTransformSession
    {
        private readonly float translationGain;
        private readonly float yawGain;
        private readonly float translationDeadZone;
        private readonly float yawDeadZone;
        private readonly float settleDistance;
        private readonly float settleYaw;
        private readonly float moveLimit;
        private readonly float translationSharpness;
        private readonly float yawSharpness;

        private bool isActive;
        private Vector3 startMidpoint;
        private Vector2 startHandAxis;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 smoothedOffset;
        private float smoothedYaw;

        public BubbleMapTransformSession(
            float translationGain,
            float yawGain,
            float translationDeadZone,
            float yawDeadZone,
            float settleDistance,
            float settleYaw,
            float moveLimit,
            float translationSharpness,
            float yawSharpness)
        {
            this.translationGain = translationGain;
            this.yawGain = yawGain;
            this.translationDeadZone = translationDeadZone;
            this.yawDeadZone = yawDeadZone;
            this.settleDistance = settleDistance;
            this.settleYaw = settleYaw;
            this.moveLimit = moveLimit;
            this.translationSharpness = translationSharpness;
            this.yawSharpness = yawSharpness;
        }

        public void Begin(UniverseView view, TwoHandGestureInput input)
        {
            if (!view.IsMini)
                return;

            isActive = true;
            startMidpoint = input.Midpoint;
            startHandAxis = input.HandAxisXZ;
            startPosition = view.CurrentPose.position;
            startRotation = view.CurrentPose.rotation;
            smoothedOffset = Vector3.zero;
            smoothedYaw = 0f;
            view.BeginBubblePreview();
        }

        public void Update(UniverseView view, TwoHandGestureInput input)
        {
            if (!isActive)
                return;

            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 targetOffset = GetTargetOffset(input);
            float targetYaw = GetYawDelta(input);
            float offsetBlend = 1f - Mathf.Exp(-translationSharpness * dt);
            float yawBlend = 1f - Mathf.Exp(-yawSharpness * dt);

            smoothedOffset = Vector3.Lerp(smoothedOffset, targetOffset, offsetBlend);
            smoothedYaw = Mathf.LerpAngle(smoothedYaw, targetYaw, yawBlend);

            Quaternion rotation = Quaternion.AngleAxis(smoothedYaw, Vector3.up) * startRotation;
            view.ApplyBubblePreview(smoothedOffset, rotation);
        }

        public void Commit(UniverseView view)
        {
            if (!isActive)
                return;

            isActive = false;
            view.CommitBubblePreview();
        }

        public void Settle(UniverseView view)
        {
            if (!isActive)
                return;

            isActive = false;
            Vector3 offset = view.CurrentPose.position - startPosition;
            offset.y = 0f;
            float yaw = Quaternion.Angle(view.CurrentPose.rotation, startRotation);
            if (offset.magnitude < settleDistance && yaw < settleYaw)
                view.CancelBubblePreview();
            else
                view.CommitBubblePreview();
        }

        private Vector3 GetTargetOffset(TwoHandGestureInput input)
        {
            Vector3 delta = input.Midpoint - startMidpoint;
            Vector3 offset = new Vector3(delta.x, 0f, delta.z) * translationGain;
            float magnitude = offset.magnitude;
            if (magnitude <= translationDeadZone)
                return Vector3.zero;

            offset *= (magnitude - translationDeadZone) / magnitude;
            return Vector3.ClampMagnitude(offset, moveLimit);
        }

        private float GetYawDelta(TwoHandGestureInput input)
        {
            if (!input.HasValidAxis || startHandAxis.sqrMagnitude < 0.0001f)
                return 0f;

            float angle = -Vector2.SignedAngle(startHandAxis, input.HandAxisXZ) * yawGain;
            float magnitude = Mathf.Abs(angle);
            return Mathf.Sign(angle) * Mathf.Max(0f, magnitude - yawDeadZone);
        }
    }
}
