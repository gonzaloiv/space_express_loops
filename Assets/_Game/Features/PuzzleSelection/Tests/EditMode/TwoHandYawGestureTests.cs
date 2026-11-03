using DigitalLove.Input;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection.Tests
{
    public class TwoHandYawGestureTests
    {
        [Test]
        public void Update_ReturnsDeltaWhenHandsRotate()
        {
            TwoHandYawGesture gesture = new(0.12f, 1f);

            Assert.AreEqual(0f, gesture.Update(State(Left(90f), Right(90f))));
            float delta = gesture.Update(State(Left(0f), Right(0f)));

            Assert.AreEqual(-90f, delta, 0.01f);
            Assert.AreEqual(-90f, gesture.AccumulatedYaw, 0.01f);
        }

        [Test]
        public void Update_RequiresSeparation()
        {
            TwoHandYawGesture gesture = new(0.12f, 1f);

            gesture.Update(State(Left(90f, 0.02f), Right(90f, 0.02f)));
            float delta = gesture.Update(State(Left(0f, 0.02f), Right(0f, 0.02f)));

            Assert.AreEqual(0f, delta, 0.01f);
            Assert.AreEqual(0f, gesture.AccumulatedYaw, 0.01f);
        }

        [Test]
        public void Update_IgnoresMostlyVerticalLine()
        {
            TwoHandYawGesture gesture = new(0.05f, 0.5f);
            TwoHandGestureState state = State(
                new Vector3(-0.05f, -0.1f, 0f), new Vector3(0.05f, 0.1f, 0f));

            gesture.Update(state);

            Assert.AreEqual(0f, gesture.AccumulatedYaw, 0.01f);
        }

        [Test]
        public void Update_ResetsWhenInactive()
        {
            TwoHandYawGesture gesture = new(0.12f, 1f);
            gesture.Update(State(Left(90f), Right(90f)));
            gesture.Update(State(Left(90f), Right(90f), false));

            Assert.AreEqual(0f, gesture.AccumulatedYaw, 0.01f);
            Assert.AreEqual(0f, gesture.Update(State(Left(0f), Right(0f))));
        }

        private static Vector3 Left(float yaw, float radius = 0.15f) =>
            new(-Mathf.Sin(yaw * Mathf.Deg2Rad) * radius, 0f, -Mathf.Cos(yaw * Mathf.Deg2Rad) * radius);

        private static Vector3 Right(float yaw, float radius = 0.15f) =>
            new(Mathf.Sin(yaw * Mathf.Deg2Rad) * radius, 0f, Mathf.Cos(yaw * Mathf.Deg2Rad) * radius);

        private static TwoHandGestureState State(
            Vector3 left, Vector3 right, bool active = true) =>
            new(
                new HandPinchState(true, left, left),
                new HandPinchState(true, right, right),
                active,
                Vector3.Distance(left, right),
                0f);
    }
}
