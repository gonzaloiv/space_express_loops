using DigitalLove.Input;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection.Tests
{
    public class RunCarouselGestureTests
    {
        private static readonly Vector3 RightAxis = Vector3.right;
        private static readonly Vector3 UpAxis = Vector3.up;

        private RunCarouselGesture gesture;

        [SetUp]
        public void SetUp() => gesture = new RunCarouselGesture(CarouselGestureSettings.Default);

        [Test]
        public void Swipe_CommitsOneStepOnRelease()
        {
            Vector3 start = new(0f, 1f, 0.5f);
            gesture.Update(Single(start, start), RightAxis, UpAxis, 0f);
            gesture.Update(Single(start + new Vector3(0.05f, 0f, 0f), start), RightAxis, UpAxis, 0.1f);

            CarouselGestureOutcome outcome = gesture.Update(None(), RightAxis, UpAxis, 0.12f);

            Assert.AreEqual(-1, outcome.FocusDelta);
        }

        [Test]
        public void Swipe_IgnoresVerticalDrag()
        {
            Vector3 start = new(0f, 1f, 0.5f);
            gesture.Update(Single(start, start), RightAxis, UpAxis, 0f);
            gesture.Update(Single(start + new Vector3(0f, 0.05f, 0f), start), RightAxis, UpAxis, 0.1f);

            CarouselGestureOutcome outcome = gesture.Update(None(), RightAxis, UpAxis, 0.12f);

            Assert.AreEqual(0, outcome.FocusDelta);
        }

        [Test]
        public void Swipe_IsCancelledWhenTheSecondHandPinches()
        {
            Vector3 start = new(0f, 1f, 0.5f);
            gesture.Update(Single(start, start), RightAxis, UpAxis, 0f);
            gesture.Update(Single(start + new Vector3(0.05f, 0f, 0f), start), RightAxis, UpAxis, 0.1f);
            gesture.Update(Both(new Vector3(-0.15f, 1f, 0.5f), new Vector3(0.15f, 1f, 0.5f), true), RightAxis, UpAxis, 0.12f);

            CarouselGestureOutcome outcome = gesture.Update(None(), RightAxis, UpAxis, 0.2f);

            Assert.AreEqual(0, outcome.FocusDelta);
        }

        [Test]
        public void Amplify_EntersOnce()
        {
            gesture.Update(Both(new Vector3(-0.1f, 1f, 0f), new Vector3(0.1f, 1f, 0f), true), RightAxis, UpAxis, 0f);

            CarouselGestureOutcome first = gesture.Update(
                Both(new Vector3(-0.15f, 1f, 0f), new Vector3(0.15f, 1f, 0f), true),
                RightAxis, UpAxis, 0.1f);
            CarouselGestureOutcome second = gesture.Update(
                Both(new Vector3(-0.15f, 1f, 0f), new Vector3(0.15f, 1f, 0f), true),
                RightAxis, UpAxis, 0.2f);

            Assert.IsTrue(first.Enter);
            Assert.IsFalse(second.Enter);
        }

        [Test]
        public void Rotation_StepsOncePerStepAngle()
        {
            Vector3 left = new(0f, 1f, 0f);
            gesture.Update(Both(left, new Vector3(0.3f, 1f, 0f), true), RightAxis, UpAxis, 0f);

            CarouselGestureOutcome first = gesture.Update(
                Both(left, new Vector3(0.23f, 1f, -0.19f), true),
                RightAxis, UpAxis, 0.1f);
            CarouselGestureOutcome second = gesture.Update(
                Both(left, new Vector3(0.23f, 1f, -0.19f), true),
                RightAxis, UpAxis, 0.2f);

            Assert.AreEqual(1, first.FocusDelta);
            Assert.AreEqual(0, second.FocusDelta);
        }

        [Test]
        public void Rotation_UsesHysteresisToReturn()
        {
            Vector3 left = new(0f, 1f, 0f);
            gesture.Update(Both(left, new Vector3(0.3f, 1f, 0f), true), RightAxis, UpAxis, 0f);
            gesture.Update(
                Both(left, new Vector3(0.23f, 1f, -0.19f), true),
                RightAxis, UpAxis, 0.1f);

            CarouselGestureOutcome held = gesture.Update(
                Both(left, new Vector3(0.26f, 1f, -0.15f), true),
                RightAxis, UpAxis, 0.2f);
            CarouselGestureOutcome returned = gesture.Update(
                Both(left, new Vector3(0.3f, 1f, 0.03f), true),
                RightAxis, UpAxis, 0.3f);

            Assert.AreEqual(0, held.FocusDelta);
            Assert.AreEqual(-1, returned.FocusDelta);
        }

        private static TwoHandGestureState Single(Vector3 point, Vector3 start) =>
            new(
                new HandPinchState(false, false, Vector3.zero, Vector3.zero),
                new HandPinchState(true, true, point, start),
                false,
                0f,
                0f);

        private static TwoHandGestureState Both(Vector3 left, Vector3 right, bool active) =>
            new(
                new HandPinchState(true, true, left, left),
                new HandPinchState(true, true, right, right),
                active,
                Vector3.Distance(left, right),
                0f);

        private static TwoHandGestureState None() =>
            new(
                new HandPinchState(false, false, Vector3.zero, Vector3.zero),
                new HandPinchState(false, false, Vector3.zero, Vector3.zero),
                false,
                0f,
                0f);
    }
}
