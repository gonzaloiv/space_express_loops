using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleSunPlacementTests
    {
        private const float SunRadius = 0.05f;
        private const float Clearance = 0.02f;

        [Test]
        public void TryHome_FlatRing_SitsInTheBandAndOutsideTheRim()
        {
            PuzzleFieldPlanet[] planets =
            {
                Planet(0.3f, 0f, 0f), Planet(-0.3f, 0f, 0f),
                Planet(0f, 0f, 0.3f), Planet(0f, 0f, -0.3f),
            };

            Assert.IsTrue(
                PuzzleSunPlacementChoice.TryHome(planets, SunRadius, Clearance, out Vector3 home));
            Assert.AreEqual(0f, home.y, 0.01f, "the flat ring's sun stays level with the planets");
            Assert.Greater(Horizontal(home), 0.3f, "the sun sits outside the planets' rim");
            AssertClearOfPlanets(home, planets);
        }

        [Test]
        public void TryHome_TallField_StaysOutsideTheRimAtEveryBandHeight()
        {
            PuzzleFieldPlanet[] planets =
            {
                Planet(0.4f, 0f, 0f), Planet(0f, 0.4f, 0f),
                Planet(-0.4f, 0f, 0f), Planet(0f, -0.4f, 0f),
            };

            Assert.IsTrue(
                PuzzleSunPlacementChoice.TryHome(planets, SunRadius, Clearance, out Vector3 home));
            Assert.That(home.y, Is.InRange(-0.4f, 0.4f), "the sun stays in the planets' height band");
            Assert.Greater(
                Horizontal(home), 0.4f, "the sun sits outside the planets' horizontal reach");
            AssertClearOfPlanets(home, planets);
        }

        private static PuzzleFieldPlanet Planet(float x, float y, float z) =>
            new(new Vector3(x, y, z), 0.04f);

        private static float Horizontal(Vector3 home) =>
            Mathf.Sqrt(home.x * home.x + home.z * home.z);

        private static void AssertClearOfPlanets(Vector3 home, PuzzleFieldPlanet[] planets)
        {
            for (int i = 0; i < planets.Length; i++)
            {
                float needed = planets[i].radius + SunRadius + Clearance;
                Assert.GreaterOrEqual(
                    Vector3.Distance(home, planets[i].position), needed,
                    $"the sun overlaps planet {i}");
            }
        }
    }
}
