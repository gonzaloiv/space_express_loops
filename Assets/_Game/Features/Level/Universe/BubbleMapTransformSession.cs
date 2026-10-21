using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class BubbleMapTransformSession
    {
        private readonly float translationGain;
        private readonly float translationDeadZone;
        private readonly float settleDistance;
        private readonly float translationSmoothSecs;
        private readonly float dragStartJumpLimit;

        private bool isActive;
        private bool hasDragStart;
        private Vector3 startMidpoint;
        private Vector3 startPosition;
        private Vector3 smoothedOffset;
        private Vector3 smoothVelocity;
        private bool dragEngaged;

        public BubbleMapTransformSession(
            float translationGain,
            float translationDeadZone,
            float settleDistance,
            float translationSmoothSecs,
            float dragStartJumpLimit)
        {
            this.translationGain = translationGain;
            this.translationDeadZone = translationDeadZone;
            this.settleDistance = settleDistance;
            this.translationSmoothSecs = translationSmoothSecs;
            this.dragStartJumpLimit = dragStartJumpLimit;
        }

        public void Begin(UniverseView view, Vector3 dragPoint)
        {
            if (!view.IsMini)
                return;

            isActive = true;
            startMidpoint = dragPoint;
            startPosition = view.CurrentPose.position;
            smoothedOffset = Vector3.zero;
            smoothVelocity = Vector3.zero;
            dragEngaged = false;
            hasDragStart = false;
            view.BeginBubblePreview();
        }

        public void Update(UniverseView view, Vector3 dragPoint)
        {
            if (!isActive)
                return;

            if (!hasDragStart)
            {
                if ((dragPoint - startMidpoint).magnitude > dragStartJumpLimit)
                {
                    startMidpoint = dragPoint;
                    startPosition = view.CurrentPose.position;
                    return;
                }

                hasDragStart = true;
            }

            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 targetOffset = GetTargetOffset(dragPoint, view.BubbleRadiusWorld * 0.4f);
            smoothedOffset = Vector3.SmoothDamp(
                smoothedOffset,
                targetOffset,
                ref smoothVelocity,
                translationSmoothSecs,
                Mathf.Infinity,
                dt);
            view.ApplyBubblePreview(smoothedOffset);
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
            if (offset.magnitude < settleDistance)
                view.CancelBubblePreview();
            else
                view.CommitBubblePreview();
        }

        private Vector3 GetTargetOffset(Vector3 dragPoint, float moveLimit)
        {
            Vector3 delta = dragPoint - startMidpoint;
            Vector3 offset = new Vector3(delta.x, 0f, delta.z) * translationGain;
            float magnitude = offset.magnitude;
            if (!dragEngaged)
            {
                if (magnitude <= translationDeadZone)
                    return Vector3.zero;

                dragEngaged = true;
            }
            else if (magnitude < translationDeadZone * 0.5f)
            {
                dragEngaged = false;
                return Vector3.zero;
            }

            magnitude = Mathf.Max(0f, magnitude - translationDeadZone);
            if (magnitude > moveLimit)
            {
                float excess = magnitude - moveLimit;
                magnitude = moveLimit + excess / (1f + excess / moveLimit);
            }

            return offset.normalized * magnitude;
        }
    }
}
