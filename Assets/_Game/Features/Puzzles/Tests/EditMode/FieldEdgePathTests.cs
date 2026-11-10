using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class FieldEdgePathTests
    {
        private const float Half = 0.5f;

        [Test]
        public void Arm_StartsFrontCentre_AndEndsBackCentre()
        {
            AssertNear(new Vector3(0f, -Half, -Half), FieldEdgePath.Point(0f, 1f, Half));
            AssertNear(new Vector3(0f, -Half, Half), FieldEdgePath.Point(1f, 1f, Half));
        }

        [Test]
        public void Arm_TurnsAtTheCorners()
        {
            AssertNear(new Vector3(Half, -Half, -Half), FieldEdgePath.Point(0.25f, 1f, Half));
            AssertNear(new Vector3(Half, -Half, Half), FieldEdgePath.Point(0.75f, 1f, Half));
        }

        [Test]
        public void LeftArm_MirrorsTheRight()
        {
            Vector3 right = FieldEdgePath.Point(0.4f, 1f, Half);
            Vector3 left = FieldEdgePath.Point(0.4f, -1f, Half);
            AssertNear(new Vector3(-right.x, right.y, right.z), left);
        }

        [Test]
        public void EqualSteps_AreEqualLengths()
        {
            float first = Vector3.Distance(FieldEdgePath.Point(0.1f, 1f, Half), FieldEdgePath.Point(0.2f, 1f, Half));
            float second = Vector3.Distance(FieldEdgePath.Point(0.4f, 1f, Half), FieldEdgePath.Point(0.5f, 1f, Half));
            Assert.AreEqual(first, second, 1e-5f);
        }

        private static void AssertNear(Vector3 expected, Vector3 actual) =>
            Assert.Less(Vector3.Distance(expected, actual), 1e-5f, $"expected {expected}, got {actual}");
    }
}
