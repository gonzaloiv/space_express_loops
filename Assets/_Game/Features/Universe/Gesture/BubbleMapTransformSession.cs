using UnityEngine;

namespace DigitalLove.Game.Universe
{
    internal enum BubbleTransformAction { None, Cancel, Commit }

    internal class BubbleMapTransformSession
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

        public static BubbleMapTransformSession FromSettings(UniverseGhostSettings settings) =>
            new(settings.translationGain,
                settings.settleDistance,
                settings.translationSmoothSecs,
                settings.dragStartJumpLimit,
                settings.minDragDistance);

        public bool Begin(BubbleViewState state, Vector3 dragPoint)
        {
            if (!state.IsBubble)
            {
                isActive = false;
                return false;
            }

            isActive = true;
            startPosition = state.Metrics.ContentPosition;
            accumulatedOffset = Vector3.zero;
            smoothedOffset = Vector3.zero;
            smoothVelocity = Vector3.zero;
            dragStarted = false;
            grabStartPoint = dragPoint;
            grabStartOffset = Vector3.zero;
            initialDragPoint = dragPoint;
            return true;
        }

        public bool TryUpdate(BubbleViewState state, Vector3 dragPoint, float dt, out Vector3 offset)
        {
            offset = Vector3.zero;
            if (!isActive)
                return false;

            if (!dragStarted)
            {
                TryStartDrag(dragPoint);
                return false;
            }

            Vector3 delta = dragPoint - grabStartPoint;
            if (delta.magnitude > dragStartJumpLimit)
            {
                RebaseGrab(dragPoint);
                return false;
            }

            offset = ApplyDrag(state, delta, dt);
            return true;
        }

        private void TryStartDrag(Vector3 dragPoint)
        {
            if ((dragPoint - initialDragPoint).magnitude < minDragDistance)
                return;

            dragStarted = true;
            RebaseGrab(dragPoint);
        }

        private void RebaseGrab(Vector3 dragPoint)
        {
            grabStartPoint = dragPoint;
            grabStartOffset = accumulatedOffset;
        }

        private Vector3 ApplyDrag(BubbleViewState state, Vector3 delta, float dt)
        {
            delta.y = 0f;
            accumulatedOffset = ApplyLimit(
                grabStartOffset + delta * translationGain,
                state.Metrics.BubbleRadiusWorld * 0.3f);

            smoothedOffset = Vector3.SmoothDamp(
                smoothedOffset,
                accumulatedOffset,
                ref smoothVelocity,
                translationSmoothSecs,
                Mathf.Infinity,
                Mathf.Max(dt, 0.0001f));
            return smoothedOffset;
        }

        public void Rebase(Vector3 dragPoint)
        {
            initialDragPoint = dragPoint;
            RebaseGrab(dragPoint);
            dragStarted = false;
        }

        public bool Commit()
        {
            if (!isActive)
                return false;

            isActive = false;
            return true;
        }

        public BubbleTransformAction Settle(Vector3 currentPosition)
        {
            if (!isActive)
                return BubbleTransformAction.None;

            isActive = false;
            Vector3 offset = currentPosition - startPosition;
            offset.y = 0f;
            return offset.magnitude < settleDistance
                ? BubbleTransformAction.Cancel
                : BubbleTransformAction.Commit;
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
