using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleClockBakeTests
    {
        private const int KnownSeed = 1;

        private SpaceShapingSettings settings;
        private PuzzleSimSettings sim;
        private PuzzlePlanetListAsset planets;

        [OneTimeSetUp]
        public void SetUp()
        {
            settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            sim = PuzzleSimSettings.From(settings);
            planets = Planets();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (planets != null)
                Object.DestroyImmediate(planets);
        }

        [Test]
        public void Bake_BakesAnOffPathClockLevel()
        {
            PuzzleBrief brief = ClockBrief(1.05f, 0.005f);
            PuzzleData baked = PuzzleFieldBake.Bake(
                planets, sim, brief, out PuzzleBakeStatus status, out PuzzleZone[] solution);

            Assert.AreEqual(PuzzleBakeStatus.Baked, status);
            PuzzleVerification check = PuzzleVerifier.Verify(baked, solution, sim);
            Assert.AreEqual(TrajectoryOutcome.Arrive, check.StartOutcome);
            Assert.IsTrue(check.StartFails, $"bare clock {check.Start.ShipTime:F3} s");
            Assert.IsTrue(check.Solved, $"clock solution {check.ShipTime:F3} s");
            Assert.IsTrue(check.Start.ShipTime < baked.tmin || check.Start.ShipTime > baked.tmax);
            Assert.IsFalse(baked.translatable);
            Assert.AreEqual(1.05f * sim.NominalSpeed, baked.launchSpeed, 1e-4f);
            Assert.Less(Vector3.Distance(baked.zoneCentre, solution[0].centre), 1e-4f);
            float run = Vector3.Distance(baked.origin, baked.destination);
            Assert.GreaterOrEqual(
                PuzzleRunGeometry.DistanceToSegment(
                    solution[0].centre, baked.origin, baked.destination),
                0.25f * run);
            Object.DestroyImmediate(baked);
        }

        [Test]
        public void Apply_UsesTheStartMargin()
        {
            PuzzleBrief brief = ClockBrief(1.05f, 0.005f);
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();

            PuzzleFieldBake.Apply(brief, puzzle, planets, sim);

            Assert.AreEqual(1.05f * sim.NominalSpeed, puzzle.launchSpeed, 1e-4f);
            Object.DestroyImmediate(puzzle);
        }

        private PuzzleBrief ClockBrief(float margin, float tightness)
        {
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = KnownSeed;
            seed.startMargin = margin;
            seed.offPath = true;
            seed.windowTightness = tightness;
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            Object.DestroyImmediate(seed);
            return brief;
        }

        private PuzzlePlanetListAsset Planets()
        {
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = KnownSeed;
            PuzzleShapeAsset shape = PuzzleArtifactFactory.Shape(seed, settings);
            PuzzlePlanetListAsset list = PuzzleArtifactFactory.Planets(shape);
            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(seed);
            return list;
        }
    }
}
