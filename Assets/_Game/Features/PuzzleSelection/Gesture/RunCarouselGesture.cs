using DigitalLove.Input;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class RunCarouselGesture
    {
        private readonly CarouselTwoHandGesture twoHand;
        private bool wasSingleHand;
        private bool wasTwoHand;
        private bool suppressSwipe;
        private bool dragging;
        private Vector3 dragStart;

        public RunCarouselGesture(CarouselGestureSettings settings)
        {
            twoHand = new CarouselTwoHandGesture(settings);
        }

        public CarouselGestureOutcome Update(in TwoHandGestureState state)
        {
            bool twoHandActive = state.IsActive;
            bool single = state.IsSingleHandPinching && !twoHandActive;

            CarouselGestureOutcome outcome;
            if (twoHandActive)
                outcome = TwoHand(state);
            else if (single)
                outcome = Drag(state);
            else
                outcome = Release();

            wasSingleHand = single;
            wasTwoHand = twoHandActive;
            return outcome;
        }

        public void Reset()
        {
            dragging = false;
            dragStart = Vector3.zero;
            twoHand.Reset();
            wasSingleHand = false;
            wasTwoHand = false;
            suppressSwipe = false;
        }

        private CarouselGestureOutcome TwoHand(in TwoHandGestureState state)
        {
            suppressSwipe = true;
            dragging = false;
            CarouselGestureOutcome outcome = twoHand.Update(state);
            return new CarouselGestureOutcome(
                outcome.Enter, outcome.ZoomProgress, outcome.OverviewProgress);
        }

        private CarouselGestureOutcome Drag(in TwoHandGestureState state)
        {
            if (wasTwoHand)
                twoHand.Reset();

            if (suppressSwipe)
                return default;

            if (!wasSingleHand || !dragging)
            {
                dragging = true;
                dragStart = state.DragPoint;
            }

            return new CarouselGestureOutcome(
                false, 0f, 0f, state.DragPoint - dragStart, true);
        }

        private CarouselGestureOutcome Release()
        {
            if (wasTwoHand)
                twoHand.Reset();

            dragging = false;
            suppressSwipe = false;
            return default;
        }
    }
}
