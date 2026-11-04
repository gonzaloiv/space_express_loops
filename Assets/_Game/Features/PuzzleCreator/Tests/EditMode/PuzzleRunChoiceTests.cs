using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleRunChoiceTests
    {
        [Test]
        public void TryPick_SunOnlyBlocker_RequiresPlanet()
        {
            PuzzleRunRequest request = Request(
                new[]
                {
                    Planet(-0.5f, 0f), Planet(0.5f, 0f), Planet(0f, 0.5f)
                },
                sunRadius: 0.06f, requirePlanet: true, allowSun: true);

            Assert.IsFalse(PuzzleRunChoice.TryPick(request, out _, out _));
        }

        [Test]
        public void TryPick_PlanetBlockerWithSunOffChord_IsAccepted()
        {
            PuzzleRunRequest request = Request(
                new[]
                {
                    Planet(-0.5f, 0.2f), Planet(0.5f, 0.2f), Planet(0f, 0.2f)
                },
                sunRadius: 0.06f, requirePlanet: true, allowSun: false);

            Assert.IsTrue(PuzzleRunChoice.TryPick(request, out Vector3 origin, out Vector3 destination));
            Assert.AreEqual(1f, Vector3.Distance(origin, destination), 1e-4f);
        }

        [Test]
        public void TryPick_SunOnChord_IsRejectedWhenDisallowed()
        {
            PuzzleRunRequest request = Request(
                new[]
                {
                    Planet(-0.5f, 0f), Planet(0.5f, 0f), Planet(0f, 0.05f)
                },
                sunRadius: 0.06f, requirePlanet: true, allowSun: false);

            Assert.IsFalse(PuzzleRunChoice.TryPick(request, out _, out _));
        }

        [Test]
        public void TryPick_SunOnChord_IsAllowedForInZone()
        {
            PuzzleRunRequest request = Request(
                new[]
                {
                    Planet(-0.5f, 0f), Planet(0.5f, 0f), Planet(0f, 0.05f)
                },
                sunRadius: 0.06f, requirePlanet: true, allowSun: true);

            Assert.IsTrue(PuzzleRunChoice.TryPick(request, out _, out _));
        }

        private static PuzzleRunRequest Request(
            PuzzleFieldPlanet[] planets, float sunRadius, bool requirePlanet, bool allowSun) => new()
        {
            Planets = planets,
            TargetRun = 1f,
            EndpointClearance = 0.03f,
            SunRadius = sunRadius,
            RequirePlanetBlocker = requirePlanet,
            AllowSunOnChord = allowSun
        };

        private static PuzzleFieldPlanet Planet(float x, float y) =>
            new(new Vector3(x, y, 0f), 0.05f);
    }
}
