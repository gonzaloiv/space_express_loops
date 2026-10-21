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

        private bool isActive;
        private Vector3 startMidpoint;
        private Vector2 startHandAxis;
        private Vector3 startPosition;
        private Quaternion startRotation;

        public BubbleMapTransformSession(
            float translationGain,
            float yawGain,
            float translationDeadZone,
            float yawDeadZone,
            float settleDistance,
            float settleYaw,
            float moveLimit)
        {
            this.translationGain = translationGain;
            this.yawGain = yawGain;
            this.translationDeadZone = translationDeadZone;
            this.yawDeadZone = yawDeadZone;
            this.settleDistance = settleDistance;
            this.settleYaw = settleYaw;
            this.moveLimit = moveLimit;
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
            view.BeginBubblePreview();
        }

        public void Update(UniverseView view, TwoHandGestureInput input)
        {
            if (!isActive)
                return;

            Vector3 delta = input.Midpoint - startMidpoint;
            Vector3 offset = new Vector3(delta.x, 0f, delta.z) * translationGain;
            if (offset.magnitude < translationDeadZone)
                offset = Vector3.zero;
            offset = Vector3.ClampMagnitude(offset, moveLimit);

            float yaw = GetYawDelta(input);
            Quaternion rotation = Quaternion.AngleAxis(yaw, Vector3.up) * startRotation;
            view.ApplyBubblePreview(startPosition + offset, rotation);
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

        private float GetYawDelta(TwoHandGestureInput input)
        {
            if (!input.HasValidAxis || startHandAxis.sqrMagnitude < 0.0001f)
                return 0f;

            float angle = -Vector2.SignedAngle(startHandAxis, input.HandAxisXZ) * yawGain;
            return Mathf.Abs(angle) < yawDeadZone ? 0f : angle;
        }
    }
}
