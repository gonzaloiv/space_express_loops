using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleSearchTests
    {
        private const int KnownSeed = 1;

        private PuzzleSimSettings settings;
        private PuzzleBrief brief;
        private PuzzleSolution solution;

        [OneTimeSetUp]
        public void FindSolution()
        {
            SpaceShapingSettings asset = Settings();
            settings = PuzzleSimSettings.From(asset);
            Object.DestroyImmediate(asset);
            PuzzleSeed source = ScriptableObject.CreateInstance<PuzzleSeed>();
            source.seed = KnownSeed;
            brief = PuzzleBrief.From(source);
            Object.DestroyImmediate(source);

            PuzzleData puzzle = Puzzle();
            bool found = PuzzleSolutionSearch.TrySelect(puzzle, settings, brief, out solution);
            Object.DestroyImmediate(puzzle);

            Assert.IsTrue(found, $"seed {KnownSeed} produced no arriving solution");
        }

        [Test]
        public void Search_HonoursTheWindow()
        {
            PuzzleData offWindow = Puzzle();
            offWindow.tmin = 0f;
            offWindow.tmax = 0.0001f;

            Assert.IsFalse(PuzzleSolutionSearch.TrySelect(offWindow, settings, brief, out _));
            Object.DestroyImmediate(offWindow);
        }

        [Test]
        public void Search_IsDeterministic()
        {
            PuzzleData puzzle = Puzzle();
            PuzzleSolutionSearch.TrySelect(puzzle, settings, brief, out PuzzleSolution first);
            PuzzleSolutionSearch.TrySelect(puzzle, settings, brief, out PuzzleSolution second);

            Assert.AreEqual(first.Zones.Length, second.Zones.Length);
            Assert.AreEqual(first.Zones[0].scale, second.Zones[0].scale, 1e-4f);
            Assert.AreEqual(first.ShipTime, second.ShipTime, 1e-4f);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void StartArrivingOutsideTheWindow_Fails()
        {
            PuzzleData puzzle = Synthetic();
            puzzle.tmin = 0f;
            puzzle.tmax = 1f;

            PuzzleVerification check = PuzzleVerifier.Verify(puzzle, null, settings);

            Assert.AreEqual(TrajectoryOutcome.Arrive, check.StartOutcome);
            Assert.IsTrue(check.StartFails);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void StartCrashing_DoesNotFail()
        {
            PuzzleData puzzle = Synthetic();
            puzzle.sources = new[] { new PuzzleSource { home = Vector3.zero, scale = 1f } };
            puzzle.tmin = 0f;
            puzzle.tmax = 10f;

            PuzzleVerification check = PuzzleVerifier.Verify(puzzle, null, settings);

            Assert.AreEqual(TrajectoryOutcome.Crash, check.StartOutcome);
            Assert.IsFalse(check.StartFails);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void FoundSolution_VerifiesGreen()
        {
            PuzzleData puzzle = Puzzle();
            puzzle.tmin = solution.ShipTime - 1f;
            puzzle.tmax = solution.ShipTime + 1f;
            puzzle.entropyBudget = solution.EntropySpent + 0.5f;

            PuzzleVerification result = PuzzleVerifier.Verify(puzzle, solution.Zones, settings);

            Assert.IsTrue(result.StartFails, $"start outcome {result.StartOutcome}");
            Assert.IsTrue(result.Solved, $"solved outcome {result.SolvedOutcome} at {result.ShipTime}");
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void FoundZone_IsInTheAuthoredRange()
        {
            PuzzleData puzzle = Puzzle();
            float size = solution.Zones[0].halfExtents.x * 2f;

            Assert.GreaterOrEqual(size, puzzle.cubeSizeMin - 1e-4f);
            Assert.LessOrEqual(size, puzzle.cubeSizeMax + 1e-4f);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void FixedBrief_KeepsTheZoneAtHome()
        {
            PuzzleData puzzle = Puzzle();

            Assert.IsFalse(brief.Translatable);
            Assert.Less(Vector3.Distance(puzzle.zoneCentre, solution.Zones[0].centre), 1e-4f);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Verify_OverBudget_IsNotSolved()
        {
            PuzzleData puzzle = Puzzle();
            puzzle.tmin = solution.ShipTime - 1f;
            puzzle.tmax = solution.ShipTime + 1f;
            puzzle.entropyBudget = 0f;

            Assert.IsFalse(PuzzleVerifier.Verify(puzzle, solution.Zones, settings).Solved);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Verify_MissingSolution_DoesNotSolve()
        {
            PuzzleData bare = Puzzle();
            bare.tmin = 0f;
            bare.tmax = 0.001f;

            Assert.IsFalse(PuzzleVerifier.Verify(bare, null, settings).Solved);
            Object.DestroyImmediate(bare);
        }

        private static PuzzleData Synthetic()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = new Vector3(-4f, 0f, 0f);
            puzzle.destination = new Vector3(4f, 0f, 0f);
            puzzle.launchSpeed = 1f;
            puzzle.sources = System.Array.Empty<PuzzleSource>();
            return puzzle;
        }

        private PuzzleData Puzzle()
        {
            SpaceShapingSettings asset = Settings();
            PuzzleSeed source = ScriptableObject.CreateInstance<PuzzleSeed>();
            source.seed = KnownSeed;
            PuzzleData created = ScriptableObject.CreateInstance<PuzzleData>();
            PuzzleShapeAsset shape = PuzzleArtifactFactory.Shape(source, asset);
            PuzzlePlanetListAsset planets = PuzzleArtifactFactory.Planets(shape);
            PuzzleFieldBake.Apply(PuzzleBrief.From(source), created, planets, PuzzleSimSettings.From(asset));
            created.tmin = 0f;
            created.tmax = created.Distance / asset.nominalSpeed * 2f;
            Object.DestroyImmediate(planets);
            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(asset);
            return created;
        }

        private static SpaceShapingSettings Settings() => ScriptableObject.CreateInstance<SpaceShapingSettings>();
    }
}
