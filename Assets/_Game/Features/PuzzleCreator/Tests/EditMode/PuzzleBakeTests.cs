using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleBakeTests
    {
        private const int SeedScan = 32;
        private const float Tightness = 1f;

        private static SpaceShapingSettings asset;
        private static PuzzleSimSettings settings;
        private static PuzzlePlanetListAsset planets;

        [OneTimeSetUp]
        public void FindSolvableField()
        {
            asset = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            settings = PuzzleSimSettings.From(asset);
            for (int seed = 1; seed <= SeedScan && planets == null; seed++)
            {
                PuzzleSeed source = ScriptableObject.CreateInstance<PuzzleSeed>();
                source.seed = seed;
                PuzzlePlanetListAsset candidate = PuzzleArtifactFactory.Planets(
                    PuzzleArtifactFactory.Shape(source, asset));
                if (PuzzleFieldBake.BakeVariants(candidate, settings, Tightness, 1).Length > 0)
                    planets = candidate;
            }

            Assert.IsNotNull(planets, "no seed in the scan produced a solvable field");
        }

        [Test]
        public void Bake_ProducesVerifiedVariants()
        {
            PuzzleData[] variants = PuzzleFieldBake.BakeVariants(planets, settings, Tightness, 2);

            Assert.IsNotEmpty(variants);
            for (int i = 0; i < variants.Length; i++)
                AssertVariant(variants[i]);
        }

        [Test]
        public void Bake_IsDeterministic()
        {
            PuzzleData[] first = PuzzleFieldBake.BakeVariants(planets, settings, Tightness, 1);
            PuzzleData[] second = PuzzleFieldBake.BakeVariants(planets, settings, Tightness, 1);

            Assert.Greater(first.Length, 0);
            Assert.AreEqual(first.Length, second.Length);
            Assert.AreEqual(first[0].tmin, second[0].tmin, 1e-4f);
            Assert.AreEqual(first[0].tmax, second[0].tmax, 1e-4f);
            Assert.AreEqual(first[0].entropyBudget, second[0].entropyBudget, 1e-4f);
        }

        private static void AssertVariant(PuzzleData variant)
        {
            PuzzleVerification check = PuzzleVerifier.Verify(variant, settings);

            Assert.IsTrue(check.StartFails, $"start outcome {check.StartOutcome}");
            Assert.IsTrue(check.Solved, $"solved outcome {check.SolvedOutcome}");
            Assert.Greater(variant.tmax, variant.tmin);
            Assert.Greater(variant.entropyBudget, 0f);
            Assert.IsNotNull(variant.solution);
            Assert.Greater(variant.solution.Length, 0);
        }
    }
}
