using UnityEngine;

namespace DigitalLove.Game.Universe
{
    internal enum BubbleTransformAction { None, Cancel, Commit }

    internal class BubbleMapTransformSession
    {
        private readonly UniverseGhostSettings settings;

        private bool isActive;
        private bool dragStarted;
        private Vector3 grabStartPoint;
        private Vector3 grabStartOffset;
        private Vector3 initialDragPoint;
        private Vector3 startPosition;
        private Vector3 accumulatedOffset;
        private Vector3 smoothedOffset;
        private Vector3 smoothVelocity;

        public BubbleMapTransformSession(UniverseGhostSettings settings)
        {
            this.settings = settings;
        }

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
            if (delta.magnitude > settings.dragStartJumpLimit)
            {
                RebaseGrab(dragPoint);
                return false;
            }

            offset = ApplyDrag(state, delta, dt);
            return true;
        }

        private void TryStartDrag(Vector3 dragPoint)
        {
            if ((dragPoint - initialDragPoint).magnitude < settings.minDragDistance)
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
                grabStartOffset + delta * settings.translationGain,
                state.Metrics.ScopeRadii,
                settings.panLimitFactor);

            smoothedOffset = Vector3.SmoothDamp(
                smoothedOffset,
                accumulatedOffset,
                ref smoothVelocity,
                settings.translationSmoothSecs,
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
            return offset.magnitude < settings.settleDistance
                ? BubbleTransformAction.Cancel
                : BubbleTransformAction.Commit;
        }

        private static Vector3 ApplyLimit(Vector3 offset, Vector3 radii, float factor)
        {
            float limit = Mathf.Max(factor, 0.0001f);
            Vector3 safe = new(
                Mathf.Max(radii.x, 0.0001f),
                Mathf.Max(radii.y, 0.0001f),
                Mathf.Max(radii.z, 0.0001f));
            Vector3 normalized = new(offset.x / safe.x, offset.y / safe.y, offset.z / safe.z);

            float magnitude = normalized.magnitude;
            if (magnitude <= limit)
                return offset;

            float scale = limit / magnitude;
            return new Vector3(
                normalized.x * scale * safe.x,
                offset.y,
                normalized.z * scale * safe.z);
        }
    }
}
