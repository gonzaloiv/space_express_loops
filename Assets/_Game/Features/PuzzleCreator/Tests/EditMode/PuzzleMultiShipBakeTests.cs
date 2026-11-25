using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleMultiShipBakeTests
    {
        private const int Seed = 13;
        private const int SunSeed = 17;

        private SpaceShapingSettings settings;
        private PuzzleSimSettings sim;
        private PuzzleBrief brief;
        private PuzzlePlanetListAsset planets;

        [OneTimeSetUp]
        public void SetUp()
        {
            settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            sim = PuzzleSimSettings.From(settings);
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = Seed;
            seed.translatable = true;
            seed.ships = 2;
            brief = PuzzleBrief.From(seed, settings);
            planets = Planets(seed);
            Object.DestroyImmediate(seed);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (planets != null)
                Object.DestroyImmediate(planets);

            if (settings != null)
                Object.DestroyImmediate(settings);
        }

        [Test]
        [Explicit]
        public void Bake_ProducesAVerifiedTwoShipPuzzle()
        {
            PuzzleMultiShipResult result = PuzzleMultiShipBake.Bake(planets, sim, brief);

            Assert.AreEqual(PuzzleBakeStatus.Baked, result.Status);
            Assert.IsNotNull(result.Puzzle);
            Assert.AreEqual(2, result.Puzzle.ships.Length);
            Assert.AreEqual(1, result.Puzzle.frames.Length);
            Assert.Greater(result.Puzzle.entropyBudget, 0f);
            Assert.IsTrue(
                new PuzzleShipScan(result.Puzzle, sim, 0f).Rescues(result.Solution, result.Puzzle.ships),
                "the baked ships are not all rescued by the solution zone");
            Object.DestroyImmediate(result.Puzzle);
        }

        [Test]
        [Explicit]
        public void OffCorridorSun_BakesWithoutASunAnswer()
        {
            SpaceShapingSettings sunSettings = Object.Instantiate(settings);
            sunSettings.sunRadius = 0.12f;
            PuzzleSimSettings sunSim = PuzzleSimSettings.From(sunSettings);
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = SunSeed;
            seed.translatable = true;
            seed.ships = 2;
            seed.hasSun = true;
            seed.sunPlacement = PuzzleSunPlacement.OffCorridor;
            seed.sunRule = PuzzleSunRule.NoSunAnswer;
            PuzzleBrief sunBrief = PuzzleBrief.From(seed, sunSettings);
            PuzzleShapeAsset shape = PuzzleArtifactFactory.Shape(seed, sunSettings);
            PuzzlePlanetListAsset sunPlanets = PuzzleArtifactFactory.Planets(shape);
            PuzzleMultiShipResult result = PuzzleMultiShipBake.Bake(sunPlanets, sunSim, sunBrief);

            Assert.AreEqual(PuzzleBakeStatus.Baked, result.Status);
            Assert.IsFalse(PuzzleSunRuleCheck.SunAnswers(result.Puzzle, sunSim));
            int sun = SunIndex(result.Puzzle.sources);
            Assert.GreaterOrEqual(sun, 0);
            PuzzleSource source = result.Puzzle.sources[sun];
            for (int i = 0; i < result.Puzzle.ships.Length; i++)
            {
                PuzzleShip ship = result.Puzzle.ships[i];
                Assert.Greater(
                    PuzzleRunGeometry.DistanceToSegment(source.home, ship.origin, ship.destination),
                    source.scale,
                    $"ship {i} runs through the sun");
            }

            Object.DestroyImmediate(result.Puzzle);
            Object.DestroyImmediate(sunPlanets);
            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(seed);
            Object.DestroyImmediate(sunSettings);
        }

        [Test]
        [Explicit]
        public void ClearBrief_AcceptsClearAndRejectsAnotherKind()
        {
            PuzzleBrief clear = brief;
            clear.Mechanism = PuzzleMechanism.Clear;
            PuzzleMultiShipResult any = PuzzleMultiShipBake.Bake(planets, sim, brief);
            PuzzleMultiShipResult role = PuzzleMultiShipBake.Bake(planets, sim, clear);

            Assert.AreEqual(PuzzleBakeStatus.Baked, any.Status);
            Assert.AreNotEqual(
                PuzzleBakeStatus.UnsupportedMechanism, role.Status, "a fleet may ask for Clear");
            if (any.Kind == PuzzleSolutionKind.Clear)
                Assert.AreEqual(PuzzleBakeStatus.Baked, role.Status);
            else
                Assert.AreEqual(PuzzleBakeStatus.WrongRole, role.Status);

            if (role.Puzzle != null)
                Object.DestroyImmediate(role.Puzzle);
            Object.DestroyImmediate(any.Puzzle);
        }

        private static int SunIndex(PuzzleSource[] sources)
        {
            for (int i = 0; i < sources.Length; i++)
                if (sources[i].centreOnly)
                    return i;

            return -1;
        }

        [Test]
        public void OneShipBrief_IsRejected()
        {
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = Seed;
            seed.ships = 1;
            PuzzleBrief single = PuzzleBrief.From(seed, settings);
            Object.DestroyImmediate(seed);

            PuzzleMultiShipResult result = PuzzleMultiShipBake.Bake(planets, sim, single);

            Assert.AreEqual(PuzzleBakeStatus.UnsupportedBrief, result.Status);
        }

        private PuzzlePlanetListAsset Planets(PuzzleSeed seed)
        {
            PuzzleShapeAsset shape = PuzzleArtifactFactory.Shape(seed, settings);
            PuzzlePlanetListAsset list = PuzzleArtifactFactory.Planets(shape);
            Object.DestroyImmediate(shape);
            return list;
        }
    }
}
