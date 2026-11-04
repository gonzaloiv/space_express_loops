using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public readonly struct CarouselGestureOutcome
    {
        public readonly int FocusDelta;
        public readonly bool Enter;
        public readonly Vector3 DragOffset;
        public readonly bool IsDragging;

        public CarouselGestureOutcome(
            int focusDelta, bool enter, Vector3 dragOffset = default, bool isDragging = false)
        {
            FocusDelta = focusDelta;
            Enter = enter;
            DragOffset = dragOffset;
            IsDragging = isDragging;
        }
    }
}
