using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleSearchTests
    {
        private const int SeedScan = 32;

        private PuzzleData puzzle;
        private PuzzleSimSettings settings;
        private PuzzleSolution solution;
        private int foundSeed;

        [OneTimeSetUp]
        public void FindSolution()
        {
            settings = PuzzleSimSettings.From(Settings());
            for (int seed = 1; seed <= SeedScan && solution.Zones == null; seed++)
            {
                puzzle = Puzzle(seed);
                PuzzleSolutionSearch.TryFind(puzzle, settings, out solution);
                if (solution.Zones != null)
                    foundSeed = seed;
            }

            Assert.IsNotNull(solution.Zones, "no seed in the scan produced an arriving solution");
        }

        [Test]
        public void Search_HonoursTheWindow()
        {
            PuzzleData offWindow = Puzzle(foundSeed);
            offWindow.tmin = 0f;
            offWindow.tmax = 0.0001f;

            Assert.IsFalse(PuzzleSolutionSearch.TryFind(offWindow, settings, out _));
        }

        [Test]
        public void Search_HonoursTheBudget()
        {
            PuzzleData noBudget = Puzzle(foundSeed);
            noBudget.tmin = 0f;
            noBudget.tmax = float.MaxValue;
            noBudget.entropyBudget = 0f;

            Assert.IsFalse(PuzzleSolutionSearch.TryFind(noBudget, settings, out _));
        }

        [Test]
        public void Search_IsDeterministic()
        {
            PuzzleSolutionSearch.TryFind(puzzle, settings, out PuzzleSolution first);
            PuzzleSolutionSearch.TryFind(puzzle, settings, out PuzzleSolution second);

            Assert.AreEqual(first.Zones.Length, second.Zones.Length);
            Assert.AreEqual(first.Zones[0].scale, second.Zones[0].scale, 1e-4f);
            Assert.AreEqual(first.ShipTime, second.ShipTime, 1e-4f);
        }

        [Test]
        public void FoundSolution_VerifiesGreen()
        {
            puzzle.solution = solution.Zones;
            puzzle.tmin = solution.ShipTime - 1f;
            puzzle.tmax = solution.ShipTime + 1f;
            puzzle.entropyBudget = solution.EntropySpent + 0.5f;

            PuzzleVerification result = PuzzleVerifier.Verify(puzzle, settings);

            Assert.IsTrue(result.StartFails, $"start outcome {result.StartOutcome}");
            Assert.IsTrue(result.Solved, $"solved outcome {result.SolvedOutcome} at {result.ShipTime}");
        }

        [Test]
        public void FoundZone_CoversTheField()
        {
            float outer = 0f;
            for (int i = 0; i < puzzle.sources.Length; i++)
                outer = Mathf.Max(outer, puzzle.sources[i].home.magnitude + puzzle.sources[i].scale * 0.5f);

            Assert.Greater(solution.Zones[0].size.x * 0.5f, outer);
        }

        [Test]
        public void Verify_OverBudget_IsNotSolved()
        {
            puzzle.solution = solution.Zones;
            puzzle.tmin = solution.ShipTime - 1f;
            puzzle.tmax = solution.ShipTime + 1f;
            puzzle.entropyBudget = 0f;

            Assert.IsFalse(PuzzleVerifier.Verify(puzzle, settings).Solved);
        }

        [Test]
        public void Verify_MissingSolution_DoesNotSolve()
        {
            PuzzleData bare = Puzzle(1);
            bare.tmin = 0f;
            bare.tmax = 0.001f;

            Assert.IsFalse(PuzzleVerifier.Verify(bare, settings).Solved);
        }

        private static PuzzleData Puzzle(int seed)
        {
            SpaceShapingSettings asset = Settings();
            PuzzleSeed source = ScriptableObject.CreateInstance<PuzzleSeed>();
            source.seed = seed;
            PuzzleData created = ScriptableObject.CreateInstance<PuzzleData>();
            PuzzleFieldBake.Apply(
                created,
                PuzzleArtifactFactory.Planets(PuzzleArtifactFactory.Shape(source, asset)),
                PuzzleSimSettings.From(asset));
            return created;
        }

        private static SpaceShapingSettings Settings() => ScriptableObject.CreateInstance<SpaceShapingSettings>();
    }
}
