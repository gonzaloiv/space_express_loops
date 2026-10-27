using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleSearchTests
    {
        private const int SeedScan = 32;

        private PuzzleData puzzle;
        private PuzzleSimSettings settings;
        private PuzzleBrief brief;
        private PuzzleSolution solution;
        private int foundSeed;

        [OneTimeSetUp]
        public void FindSolution()
        {
            settings = PuzzleSimSettings.From(Settings());
            PuzzleSeed source = ScriptableObject.CreateInstance<PuzzleSeed>();
            brief = PuzzleBrief.From(source);
            Object.DestroyImmediate(source);
            for (int seed = 1; seed <= SeedScan; seed++)
            {
                PuzzleData candidate = Puzzle(seed);
                PuzzleSolutionSearch.TrySelect(candidate, settings, brief, out solution);
                if (solution.Zones != null)
                {
                    puzzle = candidate;
                    foundSeed = seed;
                    break;
                }

                Object.DestroyImmediate(candidate);
            }

            Assert.IsNotNull(solution.Zones, "no seed in the scan produced an arriving solution");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (puzzle != null)
                Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Search_HonoursTheWindow()
        {
            PuzzleData offWindow = Puzzle(foundSeed);
            offWindow.tmin = 0f;
            offWindow.tmax = 0.0001f;

            Assert.IsFalse(PuzzleSolutionSearch.TrySelect(offWindow, settings, brief, out _));
            Object.DestroyImmediate(offWindow);
        }

        [Test]
        public void Search_IsDeterministic()
        {
            PuzzleSolutionSearch.TrySelect(puzzle, settings, brief, out PuzzleSolution first);
            PuzzleSolutionSearch.TrySelect(puzzle, settings, brief, out PuzzleSolution second);

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
        public void FoundZone_IsInTheAuthoredRange()
        {
            Assert.GreaterOrEqual(solution.Zones[0].size.x, puzzle.cubeSizeMin - 1e-4f);
            Assert.LessOrEqual(solution.Zones[0].size.x, puzzle.cubeSizeMax + 1e-4f);
        }

        [Test]
        public void FixedBrief_KeepsTheZoneAtHome()
        {
            Assert.IsFalse(brief.Translatable);
            Assert.Less(Vector3.Distance(puzzle.zoneCentre, solution.Zones[0].centre), 1e-4f);
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
            Object.DestroyImmediate(bare);
        }

        private static PuzzleData Puzzle(int seed)
        {
            SpaceShapingSettings asset = Settings();
            PuzzleSeed source = ScriptableObject.CreateInstance<PuzzleSeed>();
            source.seed = seed;
            PuzzleData created = ScriptableObject.CreateInstance<PuzzleData>();
            PuzzleShapeAsset shape = PuzzleArtifactFactory.Shape(source, asset);
            PuzzlePlanetListAsset planets = PuzzleArtifactFactory.Planets(shape);
            PuzzleFieldBake.Apply(PuzzleBrief.From(source), created, planets, PuzzleSimSettings.From(asset));
            created.tmin = 0f;
            created.tmax = created.Distance / asset.nominalSpeed * 2f;
            Object.DestroyImmediate(planets);
            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(source);
            return created;
        }

        private static SpaceShapingSettings Settings() => ScriptableObject.CreateInstance<SpaceShapingSettings>();
    }
}
