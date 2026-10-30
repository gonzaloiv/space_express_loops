using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleMultiShipBakeTests
    {
        private const int Seed = 1;

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
