using DigitalLove.Input;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection.Tests
{
    public class ScaleOutGestureTests
    {
        [Test]
        public void Update_FiresWhenHandsMoveCloser()
        {
            ScaleOutGesture gesture = new(0.08f, 0.8f);

            Assert.IsFalse(gesture.Update(State(false, 0.5f)));
            Assert.IsFalse(gesture.Update(State(true, 0.5f)));
            Assert.IsTrue(gesture.Update(State(true, 0.4f)));
        }

        [Test]
        public void Update_FiresOnceUntilReset()
        {
            ScaleOutGesture gesture = new(0.08f, 0.8f);
            gesture.Update(State(true, 0.5f));

            Assert.IsTrue(gesture.Update(State(true, 0.4f)));
            Assert.IsFalse(gesture.Update(State(true, 0.3f)));

            gesture.Reset();
            gesture.Update(State(true, 0.5f));

            Assert.IsTrue(gesture.Update(State(true, 0.4f)));
        }

        [Test]
        public void Update_IgnoresHandsMovingApart()
        {
            ScaleOutGesture gesture = new(0.08f, 0.8f);
            gesture.Update(State(true, 0.5f));

            Assert.IsFalse(gesture.Update(State(true, 0.6f)));
            Assert.IsFalse(gesture.Update(State(true, 0.7f)));
        }

        [Test]
        public void Update_RequiresRatio()
        {
            ScaleOutGesture gesture = new(0.08f, 0.8f);
            gesture.Update(State(true, 1f));

            Assert.IsFalse(gesture.Update(State(true, 0.9f)));
            Assert.IsTrue(gesture.Update(State(true, 0.79f)));
        }

        [Test]
        public void Update_RequiresDistance()
        {
            ScaleOutGesture gesture = new(0.08f, 0.8f);
            gesture.Update(State(true, 0.2f));

            Assert.IsFalse(gesture.Update(State(true, 0.15f)));
            Assert.IsTrue(gesture.Update(State(true, 0.11f)));
        }

        [Test]
        public void Update_NewGestureUsesNewStart()
        {
            ScaleOutGesture gesture = new(0.08f, 0.8f);
            gesture.Update(State(true, 1f));
            Assert.IsFalse(gesture.Update(State(true, 0.9f)));

            gesture.Update(State(false, 0.9f));
            gesture.Update(State(true, 0.2f));

            Assert.IsTrue(gesture.Update(State(true, 0.11f)));
        }

        private static TwoHandGestureState State(bool active, float distance) =>
            new(
                new HandPinchState(true, true, Vector3.zero, Vector3.zero),
                new HandPinchState(true, true, Vector3.right * distance, Vector3.right * distance),
                active,
                distance,
                0f);
    }
}
