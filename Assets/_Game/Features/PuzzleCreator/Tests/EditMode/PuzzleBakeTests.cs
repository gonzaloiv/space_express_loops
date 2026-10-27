using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleBakeTests
    {
        private const int SeedScan = 32;

        private static SpaceShapingSettings settings;
        private static PuzzleSimSettings sim;
        private static PuzzleSeed source;
        private static PuzzleBrief brief;
        private static PuzzlePlanetListAsset planets;

        [OneTimeSetUp]
        public void FindSolvableField()
        {
            settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            sim = PuzzleSimSettings.From(settings);
            source = ScriptableObject.CreateInstance<PuzzleSeed>();
            brief = PuzzleBrief.From(source);
            for (int seed = 1; seed <= SeedScan && planets == null; seed++)
            {
                source.seed = seed;
                PuzzlePlanetListAsset candidate = Planets(source);
                PuzzleData baked = PuzzleFieldBake.Bake(candidate, sim, brief, out _);
                if (baked == null)
                {
                    Object.DestroyImmediate(candidate);
                    continue;
                }

                Object.DestroyImmediate(baked);
                planets = candidate;
            }

            Assert.IsNotNull(planets, "no seed in the scan produced a solvable field");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (planets != null)
                Object.DestroyImmediate(planets);
        }

        [Test]
        public void Bake_ProducesOneVerifiedVariant()
        {
            PuzzleData variant = Bake();

            AssertVariant(variant);
            Object.DestroyImmediate(variant);
        }

        [Test]
        public void Bake_IsDeterministic()
        {
            PuzzleData first = Bake();
            PuzzleData second = Bake();

            Assert.AreEqual(first.tmin, second.tmin, 1e-4f);
            Assert.AreEqual(first.tmax, second.tmax, 1e-4f);
            Assert.AreEqual(first.entropyBudget, second.entropyBudget, 1e-4f);
            Assert.Less(Vector3.Distance(first.zoneCentre, second.zoneCentre), 1e-4f);
            Assert.AreEqual(first.solution[0].scale, second.solution[0].scale, 1e-4f);
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }

        [Test]
        public void Bake_DerivesTheWindowFromTheRun()
        {
            PuzzleData variant = Bake();
            float cross = variant.Distance / sim.NominalSpeed;
            float tightness = brief.WindowTightness;

            Assert.AreEqual(cross * (1f - tightness), variant.tmin, 1e-3f);
            Assert.AreEqual(cross * (1f + tightness), variant.tmax, 1e-3f);

            PuzzleVerification run = PuzzleVerifier.Verify(variant, sim);
            Assert.GreaterOrEqual(run.ShipTime, variant.tmin - 1e-3f);
            Assert.LessOrEqual(run.ShipTime, variant.tmax + 1e-3f);
            Object.DestroyImmediate(variant);
        }

        [Test]
        public void Bake_FixedAtTheBakedZone()
        {
            PuzzleData variant = Bake();

            Assert.IsFalse(variant.translatable);
            Assert.AreEqual(1, variant.moves);
            Assert.Less(Vector3.Distance(variant.zoneCentre, variant.solution[0].centre), 1e-4f);
            Object.DestroyImmediate(variant);
        }

        [Test]
        public void Brief_CarriesTheSeedConditions()
        {
            PuzzleSeed authored = ScriptableObject.CreateInstance<PuzzleSeed>();
            authored.translatable = true;
            authored.moves = 2;
            authored.resizable = true;
            authored.rotate = true;
            authored.windowTightness = 0.25f;

            PuzzleBrief mapped = PuzzleBrief.From(authored);

            Assert.IsTrue(mapped.Translatable);
            Assert.AreEqual(2, mapped.Moves);
            Assert.IsTrue(mapped.Resizable);
            Assert.IsTrue(mapped.Rotate);
            Assert.AreEqual(0.25f, mapped.WindowTightness, 1e-4f);
            Assert.IsFalse(mapped.SupportsSingleZone);
            Object.DestroyImmediate(authored);
        }

        [Test]
        public void Bake_RejectsMultiZone()
        {
            PuzzleBrief multi = brief;
            multi.Moves = 2;

            PuzzleData rejected = PuzzleFieldBake.Bake(planets, sim, multi, out PuzzleBakeStatus status);

            Assert.IsNull(rejected);
            Assert.AreEqual(PuzzleBakeStatus.UnsupportedBrief, status);
        }

        [Test]
        public void Bake_RejectsRotate()
        {
            PuzzleBrief rotating = brief;
            rotating.Rotate = true;

            PuzzleData rejected = PuzzleFieldBake.Bake(planets, sim, rotating, out PuzzleBakeStatus status);

            Assert.IsNull(rejected);
            Assert.AreEqual(PuzzleBakeStatus.UnsupportedBrief, status);
        }

        [Test]
        public void Bake_TranslatableMovesTheZoneOffHome()
        {
            PuzzleBrief movable = brief;
            movable.Translatable = true;
            for (int seed = 1; seed <= SeedScan; seed++)
            {
                source.seed = seed;
                PuzzlePlanetListAsset candidate = Planets(source);
                PuzzleData baked = PuzzleFieldBake.Bake(candidate, sim, movable, out _);
                Object.DestroyImmediate(candidate);
                if (baked == null)
                    continue;

                Assert.IsTrue(baked.translatable);
                Vector3 home = (baked.origin + baked.destination) * 0.5f;
                Assert.Greater(Vector3.Distance(home, baked.solution[0].centre), 1e-4f);
                Object.DestroyImmediate(baked);
                return;
            }

            Assert.Fail("no seed in the scan produced a translatable field");
        }

        private static PuzzleData Bake()
        {
            PuzzleData variant = PuzzleFieldBake.Bake(planets, sim, brief, out PuzzleBakeStatus status);

            Assert.AreEqual(PuzzleBakeStatus.Baked, status);
            Assert.IsNotNull(variant);
            return variant;
        }

        private static void AssertVariant(PuzzleData variant)
        {
            PuzzleVerification check = PuzzleVerifier.Verify(variant, sim);

            Assert.IsTrue(check.StartFails, $"start outcome {check.StartOutcome}");
            Assert.IsTrue(check.Solved, $"solved outcome {check.SolvedOutcome}");
            Assert.Greater(check.StartMargin, 0f);
            Assert.Greater(variant.tmax, variant.tmin);
            Assert.Greater(variant.entropyBudget, 0f);
            Assert.IsNotNull(variant.solution);
            Assert.AreEqual(1, variant.solution.Length);
            Assert.GreaterOrEqual(variant.cubeSize, variant.cubeSizeMin - 1e-4f);
            Assert.LessOrEqual(variant.cubeSize, variant.cubeSizeMax + 1e-4f);
            Assert.LessOrEqual(check.EntropySpent, variant.entropyBudget / 3f + 1e-3f);
        }

        private static PuzzlePlanetListAsset Planets(PuzzleSeed seed)
        {
            PuzzleShapeAsset shape = PuzzleArtifactFactory.Shape(seed, settings);
            PuzzlePlanetListAsset planets = PuzzleArtifactFactory.Planets(shape);
            Object.DestroyImmediate(shape);
            return planets;
        }
    }
}
