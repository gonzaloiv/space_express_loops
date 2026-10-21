using UnityEngine;

namespace DigitalLove.Game.Universe
{
    public class BubbleMapTransformSession
    {
        private readonly float translationGain;
        private readonly float settleDistance;
        private readonly float translationSmoothSecs;
        private readonly float dragStartJumpLimit;
        private readonly float minDragDistance;

        private bool isActive;
        private bool dragStarted;
        private Vector3 grabStartPoint;
        private Vector3 grabStartOffset;
        private Vector3 initialDragPoint;
        private Vector3 startPosition;
        private Vector3 accumulatedOffset;
        private Vector3 smoothedOffset;
        private Vector3 smoothVelocity;

        public BubbleMapTransformSession(
            float translationGain,
            float settleDistance,
            float translationSmoothSecs,
            float dragStartJumpLimit,
            float minDragDistance)
        {
            this.translationGain = translationGain;
            this.settleDistance = settleDistance;
            this.translationSmoothSecs = translationSmoothSecs;
            this.dragStartJumpLimit = dragStartJumpLimit;
            this.minDragDistance = minDragDistance;
        }

        public void Begin(UniverseView view, Vector3 dragPoint)
        {
            if (!view.IsMini)
                return;

            isActive = true;
            startPosition = view.CurrentPose.position;
            accumulatedOffset = Vector3.zero;
            smoothedOffset = Vector3.zero;
            smoothVelocity = Vector3.zero;
            dragStarted = false;
            grabStartPoint = dragPoint;
            grabStartOffset = Vector3.zero;
            initialDragPoint = dragPoint;
            view.BeginBubblePreview();
        }

        public void Update(UniverseView view, Vector3 dragPoint)
        {
            if (!isActive)
                return;

            if (!dragStarted)
            {
                if ((dragPoint - initialDragPoint).magnitude < minDragDistance)
                    return;

                dragStarted = true;
                grabStartPoint = dragPoint;
                grabStartOffset = accumulatedOffset;
                return;
            }

            Vector3 delta = dragPoint - grabStartPoint;
            if (delta.magnitude > dragStartJumpLimit)
            {
                grabStartPoint = dragPoint;
                grabStartOffset = accumulatedOffset;
                return;
            }

            delta.y = 0f;
            accumulatedOffset = ApplyLimit(
                grabStartOffset + delta * translationGain,
                view.BubbleRadiusWorld * 0.3f);

            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            smoothedOffset = Vector3.SmoothDamp(
                smoothedOffset,
                accumulatedOffset,
                ref smoothVelocity,
                translationSmoothSecs,
                Mathf.Infinity,
                dt);
            view.ApplyBubbleDrag(smoothedOffset);
        }

        public void Rebase(Vector3 dragPoint)
        {
            initialDragPoint = dragPoint;
            grabStartPoint = dragPoint;
            grabStartOffset = accumulatedOffset;
            dragStarted = false;
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

        private static Vector3 ApplyLimit(Vector3 offset, float limit)
        {
            float magnitude = offset.magnitude;
            if (magnitude <= limit)
                return offset;

            float excess = magnitude - limit;
            float softened = limit + excess / (1f + excess / Mathf.Max(limit, 0.0001f));
            return offset.normalized * softened;
        }
    }
}
