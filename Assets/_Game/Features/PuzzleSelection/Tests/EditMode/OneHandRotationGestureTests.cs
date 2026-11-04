using DigitalLove.Input;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection.Tests
{
    public class OneHandRotationGestureTests
    {
        [Test]
        public void Update_EmitsStepWhenDraggingRight()
        {
            OneHandRotationGesture gesture = new(0.08f);

            Assert.AreEqual(0, gesture.Update(Left(0f), Vector3.right));
            Assert.AreEqual(1, gesture.Update(Left(0.08f), Vector3.right));
            Assert.AreEqual(0, gesture.Update(Left(0.08f), Vector3.right));
        }

        [Test]
        public void Update_EmitsNegativeStepWhenDraggingLeft()
        {
            OneHandRotationGesture gesture = new(0.08f);

            gesture.Update(Left(0f), Vector3.right);

            Assert.AreEqual(-1, gesture.Update(Left(-0.08f), Vector3.right));
        }

        [Test]
        public void Update_IgnoresTwoHandPinch()
        {
            OneHandRotationGesture gesture = new(0.08f);
            gesture.Update(Left(0f), Vector3.right);

            Assert.AreEqual(0, gesture.Update(Both(), Vector3.right));
            Assert.AreEqual(0, gesture.Update(Left(0.16f), Vector3.right));
        }

        [Test]
        public void Update_ResetsWhenPinchEnds()
        {
            OneHandRotationGesture gesture = new(0.08f);
            gesture.Update(Left(0f), Vector3.right);
            gesture.Update(Left(0.08f), Vector3.right);

            gesture.Update(None(), Vector3.right);
            gesture.Update(Left(0.5f), Vector3.right);

            Assert.AreEqual(1, gesture.Update(Left(0.58f), Vector3.right));
        }

        [Test]
        public void Update_ResetsWhenTheHandChanges()
        {
            OneHandRotationGesture gesture = new(0.08f);
            gesture.Update(Left(0f), Vector3.right);
            gesture.Update(Left(0.08f), Vector3.right);

            Assert.AreEqual(0, gesture.Update(Right(1f), Vector3.right));
            Assert.AreEqual(1, gesture.Update(Right(1.08f), Vector3.right));
        }

        private static TwoHandGestureState Left(float x) =>
            State(true, false, new Vector3(x, 1f, 0f), Vector3.zero);

        private static TwoHandGestureState Right(float x) =>
            State(false, true, Vector3.zero, new Vector3(x, 1f, 0f));

        private static TwoHandGestureState Both() =>
            State(true, true, Vector3.zero, Vector3.right * 0.2f);

        private static TwoHandGestureState None() =>
            State(false, false, Vector3.zero, Vector3.zero);

        private static TwoHandGestureState State(
            bool leftPinch, bool rightPinch, Vector3 left, Vector3 right) =>
            new(
                new HandPinchState(leftPinch, left, left),
                new HandPinchState(rightPinch, right, right),
                false,
                Vector3.Distance(left, right),
                0f);
    }
}
