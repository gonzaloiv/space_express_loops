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
            PuzzleSeed source = ScriptableObject.CreateInstance<PuzzleSeed>();
            source.seed = KnownSeed;
            brief = PuzzleBrief.From(source, asset);
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(asset);

            PuzzleData puzzle = Puzzle();
            bool found = PuzzleSolutionSearch.TrySelect(puzzle, settings, brief, out solution);
            Object.DestroyImmediate(puzzle);

            Assert.IsTrue(found, $"seed {KnownSeed} produced no arriving solution");
        }

        [Test]
        public void Search_IsDeterministic()
        {
            PuzzleData puzzle = Puzzle();
            PuzzleSolutionSearch.TrySelect(puzzle, settings, brief, out PuzzleSolution first);
            PuzzleSolutionSearch.TrySelect(puzzle, settings, brief, out PuzzleSolution second);

            Assert.AreEqual(first.Zones.Length, second.Zones.Length);
            Assert.AreEqual(first.Zones[0].scale, second.Zones[0].scale, 1e-4f);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void StartCrashing_Fails()
        {
            PuzzleData puzzle = Synthetic();
            puzzle.sources = new[] { new PuzzleSource { home = Vector3.zero, scale = 1f } };

            PuzzleVerification check = PuzzleVerifier.Verify(puzzle, null, settings);

            Assert.AreEqual(TrajectoryOutcome.Crash, check.StartOutcome);
            Assert.IsTrue(check.StartFails);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void FoundSolution_VerifiesGreen()
        {
            PuzzleData puzzle = Puzzle();
            puzzle.entropyBudget = solution.EntropySpent + 0.5f;

            PuzzleVerification result = PuzzleVerifier.Verify(puzzle, solution.Zones, settings);

            Assert.IsTrue(result.StartFails, $"start outcome {result.StartOutcome}");
            Assert.IsTrue(result.Solved, $"solved outcome {result.SolvedOutcome}");
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
        public void FixedBrief_UsesAContentCentre()
        {
            PuzzleData puzzle = Puzzle();

            Assert.IsFalse(brief.Translatable);
            Assert.IsTrue(ContainsSource(puzzle, solution.Zones[0]));
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Verify_OverBudget_IsNotSolved()
        {
            PuzzleData puzzle = Puzzle();
            puzzle.entropyBudget = 0f;

            Assert.IsFalse(PuzzleVerifier.Verify(puzzle, solution.Zones, settings).Solved);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Verify_MissingSolution_DoesNotSolve()
        {
            PuzzleData bare = Puzzle();

            Assert.IsFalse(PuzzleVerifier.Verify(bare, null, settings).Solved);
            Object.DestroyImmediate(bare);
        }

        private static bool ContainsSource(PuzzleData puzzle, PuzzleZone zone)
        {
            for (int i = 0; i < puzzle.sources.Length; i++)
            {
                Vector3 offset = puzzle.sources[i].home - zone.centre;
                if (Mathf.Abs(offset.x) <= zone.halfExtents.x
                    && Mathf.Abs(offset.y) <= zone.halfExtents.y
                    && Mathf.Abs(offset.z) <= zone.halfExtents.z)
                    return true;
            }

            return false;
        }

        private static PuzzleData Synthetic()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = new Vector3(-4f, 0f, 0f);
            puzzle.destination = new Vector3(4f, 0f, 0f);
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
            PuzzleFieldBake.Apply(brief, created, planets, PuzzleSimSettings.From(asset));
            Object.DestroyImmediate(planets);
            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(asset);
            return created;
        }

        private static SpaceShapingSettings Settings() => ScriptableObject.CreateInstance<SpaceShapingSettings>();
    }
}
