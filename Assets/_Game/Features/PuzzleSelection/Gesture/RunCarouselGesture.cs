using DigitalLove.Input;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class RunCarouselGesture
    {
        private readonly CarouselGestureSettings settings;
        private readonly CarouselTwoHandGesture twoHand;
        private bool wasSingleHand;
        private bool wasTwoHand;
        private bool suppressSwipe;
        private bool swiping;
        private Vector3 swipeStart;
        private Vector3 swipeLast;
        private float swipeStartSecs;

        public RunCarouselGesture(CarouselGestureSettings settings)
        {
            this.settings = settings;
            twoHand = new CarouselTwoHandGesture(settings);
        }

        public CarouselGestureOutcome Update(
            in TwoHandGestureState state, Vector3 rightAxis, Vector3 upAxis, float nowSecs)
        {
            bool twoHand = state.IsActive;
            bool single = state.IsSingleHandPinching && !twoHand;
            int focusDelta = 0;
            bool enter = false;
            float speed = 0f;

            if (twoHand)
                UpdateTwoHand(state, out focusDelta, out enter);
            else if (single)
                UpdateSingleHand(state, nowSecs);
            else
            {
                CarouselGestureOutcome release = Release(rightAxis, upAxis, nowSecs);
                focusDelta = release.FocusDelta;
                speed = release.Speed;
            }

            wasSingleHand = single;
            wasTwoHand = twoHand;
            return new CarouselGestureOutcome(focusDelta, enter, speed);
        }

        public void Reset()
        {
            ResetSwipe();
            twoHand.Reset();
            wasSingleHand = false;
            wasTwoHand = false;
            suppressSwipe = false;
        }

        private void UpdateTwoHand(
            in TwoHandGestureState state, out int focusDelta, out bool enter)
        {
            suppressSwipe = true;
            if (wasSingleHand)
                ResetSwipe();

            CarouselGestureOutcome outcome = twoHand.Update(state);
            focusDelta = outcome.FocusDelta;
            enter = outcome.Enter;
        }

        private void UpdateSingleHand(in TwoHandGestureState state, float nowSecs)
        {
            if (wasTwoHand)
                twoHand.Reset();
            if (suppressSwipe)
                return;

            if (!wasSingleHand || !swiping)
                BeginSwipe(state, nowSecs);

            swipeLast = state.DragPoint;
        }

        private CarouselGestureOutcome Release(Vector3 rightAxis, Vector3 upAxis, float nowSecs)
        {
            CarouselGestureOutcome outcome = default;
            if (wasSingleHand && swiping)
                outcome = CommitSwipe(rightAxis, upAxis, nowSecs);

            if (wasTwoHand)
                twoHand.Reset();

            suppressSwipe = false;
            return outcome;
        }

        private void BeginSwipe(in TwoHandGestureState state, float nowSecs)
        {
            swiping = true;
            swipeStart = state.DragPoint;
            swipeLast = swipeStart;
            swipeStartSecs = nowSecs;
        }

        private CarouselGestureOutcome CommitSwipe(
            Vector3 rightAxis, Vector3 upAxis, float nowSecs)
        {
            Vector3 delta = swipeLast - swipeStart;
            Vector3 right = SafeAxis(rightAxis, Vector3.right);
            Vector3 up = SafeAxis(upAxis, Vector3.up);
            float horizontal = Vector3.Dot(delta, right);
            float vertical = Vector3.Dot(delta, up);
            float duration = Mathf.Max(nowSecs - swipeStartSecs, 0.016f);
            float speed = delta.magnitude / duration;
            bool horizontalEnough =
                Mathf.Abs(horizontal) >= settings.SwipeHorizontalDominance * Mathf.Abs(vertical);
            bool moves = delta.magnitude >= settings.MinSwipeDistance
                || speed >= settings.MinSwipeSpeed;

            ResetSwipe();
            if (!horizontalEnough || !moves)
                return default;

            int focusDelta = horizontal > 0f ? -1 : 1;
            return new CarouselGestureOutcome(focusDelta, false, speed);
        }

        private void ResetSwipe()
        {
            swiping = false;
            swipeStart = Vector3.zero;
            swipeLast = Vector3.zero;
            swipeStartSecs = 0f;
        }

        private static Vector3 SafeAxis(Vector3 axis, Vector3 fallback) =>
            axis.sqrMagnitude > 0.000001f ? axis.normalized : fallback;
    }
}
