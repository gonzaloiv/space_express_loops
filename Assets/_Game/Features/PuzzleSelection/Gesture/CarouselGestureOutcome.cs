using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public readonly struct CarouselGestureOutcome
    {
        public readonly bool Enter;
        public readonly float ZoomProgress;
        public readonly float OverviewProgress;
        public readonly Vector3 DragOffset;
        public readonly bool IsDragging;

        public CarouselGestureOutcome(
            bool enter,
            float zoomProgress = 0f,
            float overviewProgress = 0f,
            Vector3 dragOffset = default,
            bool isDragging = false)
        {
            Enter = enter;
            ZoomProgress = zoomProgress;
            OverviewProgress = overviewProgress;
            DragOffset = dragOffset;
            IsDragging = isDragging;
        }
    }
}
