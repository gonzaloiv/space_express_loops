using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.Placement.Tests.EditMode
{
    public class SeatPoseTests
    {
        private static readonly Vector3 Head = new(0f, 1.2f, 0f);
        private static readonly SeatSettings Settings = SeatSettings.Default;

        [Test]
        public void NoHands_PlacesAtReachAndTheDefaultDrop()
        {
            SeatPose pose = SeatPose.From(Head, Quaternion.identity, null, null, Settings);

            AssertNear(new Vector3(0f, 1.2f - Settings.defaultDrop, Settings.distance), pose.Position);
            AssertNear(Vector3.forward, pose.Forward);
        }

        [Test]
        public void LookingDown_KeepsTheFieldLevelInFront()
        {
            Quaternion down = Quaternion.Euler(60f, 90f, 0f);

            SeatPose pose = SeatPose.From(Head, down, null, null, Settings);

            AssertNear(Vector3.right, pose.Forward);
            AssertNear(new Vector3(Settings.distance, 1.2f - Settings.defaultDrop, 0f), pose.Position);
        }

        [Test]
        public void LookingStraightDown_StillFindsAForward()
        {
            SeatPose pose = SeatPose.From(Head, Quaternion.Euler(90f, 0f, 0f), null, null, Settings);

            AssertNear(Vector3.forward, pose.Forward);
        }

        [Test]
        public void HandsRestingOnTheDesk_SetTheHeight()
        {
            Vector3 left = new(-0.15f, 0.75f, 0.35f);
            Vector3 right = new(0.15f, 0.75f, 0.35f);

            SeatPose pose = SeatPose.From(Head, Quaternion.identity, left, right, Settings);

            Assert.AreEqual(0.75f + Settings.aboveHands, pose.Position.y, 1e-4f);
        }

        [Test]
        public void RaisedOrFarHands_AreIgnored()
        {
            Vector3 raised = new(0f, 1.1f, 0.3f);
            Vector3 behind = new(0f, 0.75f, -0.3f);

            SeatPose pose = SeatPose.From(Head, Quaternion.identity, raised, behind, Settings);

            Assert.AreEqual(1.2f - Settings.defaultDrop, pose.Position.y, 1e-4f);
        }

        [Test]
        public void TheHeight_StaysInTheComfortBand()
        {
            Vector3 low = new(0f, 0.45f, 0.3f);

            SeatPose pose = SeatPose.From(Head, Quaternion.identity, low, null, Settings);

            Assert.AreEqual(1.2f - Settings.maxDrop, pose.Position.y, 1e-4f);
        }

        private static void AssertNear(Vector3 expected, Vector3 actual) =>
            Assert.Less(Vector3.Distance(expected, actual), 1e-4f, $"expected {expected}, got {actual}");
    }
}
