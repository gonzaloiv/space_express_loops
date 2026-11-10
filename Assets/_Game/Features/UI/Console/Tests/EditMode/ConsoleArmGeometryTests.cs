using DigitalLove.Game.UI.Console;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.UI.Console.Tests.EditMode
{
    public class ConsoleArmGeometryTests
    {
        private const float SmallestHalf = 0.227f;
        private const float LargestHalf = 0.308f;
        private const float MapHalf = 0.084f;

        [Test]
        public void Corner_IsTheGridsBottomLeftBack()
        {
            Vector3 centre = new(0.03f, -0.02f, 0.02f);
            Vector3 corner = ConsoleArmGeometry.Corner(centre, LargestHalf);
            AssertNear(new Vector3(centre.x - LargestHalf, centre.y - LargestHalf, centre.z + LargestHalf), corner);
        }

        [Test]
        public void DiagonalArm_RunsOutLeftAndUpAt45Degrees()
        {
            Vector3 corner = ConsoleArmGeometry.Corner(Vector3.zero, LargestHalf);
            Vector3 end = ConsoleArmGeometry.DiagonalEnd(corner, Quaternion.identity, 0.24f);
            Vector3 delta = end - corner;
            Assert.AreEqual(-delta.y, delta.x, 1e-5f, "the arm is 45° left and up");
            Assert.AreEqual(0f, delta.z, 1e-5f, "the arm stays in the field's vertical plane");
            Assert.Less(delta.x, 0f);
            Assert.Greater(delta.y, 0f);
        }

        [Test]
        public void VerticalArm_RisesStraightUp_JustOutsideTheCorner()
        {
            Vector3 centre = new(0.02f, 0.01f, 0.03f);
            Vector3 corner = ConsoleArmGeometry.Corner(centre, SmallestHalf);
            Vector3 foot = ConsoleArmGeometry.VerticalFoot(corner, Quaternion.identity, 0.02f);
            Vector3 end = ConsoleArmGeometry.VerticalEnd(corner, Quaternion.identity, 0.28f, 0.02f);
            AssertNear(new Vector3(corner.x - 0.02f, corner.y, corner.z), foot);
            AssertNear(new Vector3(corner.x - 0.02f, corner.y + 0.28f, corner.z), end);
            Assert.AreEqual(foot.x, end.x, 1e-5f);
        }

        [Test]
        public void Window_HangsLeftAndBelowItsInnerTop()
        {
            Vector3 inner = new(-0.3f, 0.1f, 0.7f);
            Vector3 centre = ConsoleArmGeometry.WindowCentre(inner, Quaternion.identity, new Vector2(0.26f, 0.23f));
            AssertNear(new Vector3(inner.x - 0.13f, inner.y - 0.115f, inner.z), centre);
        }

        [Test]
        public void BothArmEnds_StayLeftOfTheGridEdge_ForEveryFrame()
        {
            float[] halves = { SmallestHalf, LargestHalf, MapHalf };
            for (int i = 0; i < halves.Length; i++)
            {
                Vector3 corner = ConsoleArmGeometry.Corner(Vector3.zero, halves[i]);
                Vector3 card = ConsoleArmGeometry.DiagonalEnd(corner, Quaternion.identity, 0.24f);
                Vector3 jim = ConsoleArmGeometry.VerticalEnd(corner, Quaternion.identity, 0.28f, 0.02f);
                Assert.Less(card.x, corner.x, $"card arm for half {halves[i]}");
                Assert.Less(jim.x, corner.x, $"jim arm for half {halves[i]}");
                Assert.Greater(card.y, corner.y, $"card arm for half {halves[i]}");
                Assert.Greater(jim.y, corner.y, $"jim arm for half {halves[i]}");
            }
        }

        private static void AssertNear(Vector3 expected, Vector3 actual) =>
            Assert.Less(Vector3.Distance(expected, actual), 1e-5f, $"expected {expected}, got {actual}");
    }
}
