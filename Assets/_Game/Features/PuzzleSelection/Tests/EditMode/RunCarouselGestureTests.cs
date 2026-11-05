using DigitalLove.Input;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection.Tests
{
    public class RunCarouselGestureTests
    {
        private static readonly Vector3 Start = new(0f, 1f, 0.5f);

        private RunCarouselGesture gesture;

        [SetUp]
        public void SetUp() => gesture = new RunCarouselGesture(CarouselGestureSettings.Default);

        [Test]
        public void Drag_ReportsTheMapOffsetUntilRelease()
        {
            gesture.Update(Single(Start));

            CarouselGestureOutcome drag = gesture.Update(
                Single(Start + new Vector3(0.1f, 0f, 0f)));

            Assert.IsTrue(drag.IsDragging);
            Assert.AreEqual(0.1f, drag.DragOffset.x, 0.0001f);
        }

        [Test]
        public void Drag_IsCancelledByTwoHandPinch()
        {
            gesture.Update(Single(Start));
            gesture.Update(Single(Start + new Vector3(0.1f, 0f, 0f)));
            gesture.Update(Both(new Vector3(-0.1f, 1f, 0f), new Vector3(0.1f, 1f, 0f), true));

            CarouselGestureOutcome outcome = gesture.Update(None());

            Assert.IsFalse(outcome.IsDragging);
        }

        [Test]
        public void ScaleIn_ReportsZoomProgressAndEntersAtThreshold()
        {
            gesture.Update(Both(new Vector3(-0.1f, 1f, 0f), new Vector3(0.1f, 1f, 0f), true));

            CarouselGestureOutcome half = gesture.Update(
                Both(new Vector3(-0.125f, 1f, 0f), new Vector3(0.125f, 1f, 0f), true));
            CarouselGestureOutcome full = gesture.Update(
                Both(new Vector3(-0.15f, 1f, 0f), new Vector3(0.15f, 1f, 0f), true));

            Assert.AreEqual(0.5f, half.ZoomProgress, 0.01f);
            Assert.IsFalse(half.Enter);
            Assert.AreEqual(1f, full.ZoomProgress, 0.01f);
            Assert.IsTrue(full.Enter);
        }

        [Test]
        public void ScaleIn_EntersOnce()
        {
            gesture.Update(Both(new Vector3(-0.1f, 1f, 0f), new Vector3(0.1f, 1f, 0f), true));

            CarouselGestureOutcome first = gesture.Update(
                Both(new Vector3(-0.15f, 1f, 0f), new Vector3(0.15f, 1f, 0f), true));
            CarouselGestureOutcome second = gesture.Update(
                Both(new Vector3(-0.15f, 1f, 0f), new Vector3(0.15f, 1f, 0f), true));

            Assert.IsTrue(first.Enter);
            Assert.IsFalse(second.Enter);
        }

        [Test]
        public void ScaleOut_ReportsOverviewProgress()
        {
            gesture.Update(Both(new Vector3(-0.15f, 1f, 0f), new Vector3(0.15f, 1f, 0f), true));

            CarouselGestureOutcome outcome = gesture.Update(
                Both(new Vector3(-0.1f, 1f, 0f), new Vector3(0.1f, 1f, 0f), true));

            Assert.Greater(outcome.OverviewProgress, 0.5f);
            Assert.AreEqual(0f, outcome.ZoomProgress, 0.01f);
            Assert.IsFalse(outcome.Enter);
        }

        private static TwoHandGestureState Single(Vector3 point) =>
            new(
                new HandPinchState(false, Vector3.zero, Vector3.zero),
                new HandPinchState(true, point, Start),
                false,
                0f,
                0f);

        private static TwoHandGestureState Both(Vector3 left, Vector3 right, bool active) =>
            new(
                new HandPinchState(true, left, left),
                new HandPinchState(true, right, right),
                active,
                Vector3.Distance(left, right),
                0f);

        private static TwoHandGestureState None() =>
            new(
                new HandPinchState(false, Vector3.zero, Vector3.zero),
                new HandPinchState(false, Vector3.zero, Vector3.zero),
                false,
                0f,
                0f);
    }
}
