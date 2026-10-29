using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleBakeTests
    {
        private const int KnownSeed = 1;

        private static SpaceShapingSettings settings;
        private static PuzzleSimSettings sim;
        private static PuzzleBrief brief;
        private static PuzzlePlanetListAsset planets;

        [OneTimeSetUp]
        public void FindSolvableField()
        {
            settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            sim = PuzzleSimSettings.From(settings);
            PuzzleSeed source = ScriptableObject.CreateInstance<PuzzleSeed>();
            source.seed = KnownSeed;
            brief = PuzzleBrief.From(source, settings);
            Object.DestroyImmediate(source);

            planets = Planets();
            PuzzleData baked = PuzzleFieldBake.Bake(
                planets, sim, brief, out PuzzleBakeStatus status, out _);
            Assert.AreEqual(PuzzleBakeStatus.Baked, status, $"seed {KnownSeed} no longer bakes");
            Assert.IsNotNull(baked);
            Object.DestroyImmediate(baked);
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
            PuzzleData variant = Bake(out PuzzleZone[] solution);

            AssertVariant(variant, solution);
            Object.DestroyImmediate(variant);
        }

        [Test]
        public void Bake_IsDeterministic()
        {
            PuzzleData first = Bake(out PuzzleZone[] firstSolution);
            PuzzleData second = Bake(out PuzzleZone[] secondSolution);

            Assert.AreEqual(first.tmin, second.tmin, 1e-4f);
            Assert.AreEqual(first.tmax, second.tmax, 1e-4f);
            Assert.AreEqual(first.entropyBudget, second.entropyBudget, 1e-4f);
            Assert.Less(Vector3.Distance(first.zoneCentre, second.zoneCentre), 1e-4f);
            Assert.AreEqual(firstSolution[0].scale, secondSolution[0].scale, 1e-4f);
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }

        [Test]
        public void Bake_DerivesTheWindowFromTheProperCrossing()
        {
            PuzzleData variant = Bake(out PuzzleZone[] solution);

            Assert.IsTrue(PuzzleWindow.TryCrossing(variant, sim, out float crossing));
            float halfWidth = brief.WindowTightness * crossing;
            Assert.AreEqual(crossing - halfWidth, variant.tmin, 1e-3f);
            Assert.AreEqual(crossing + halfWidth, variant.tmax, 1e-3f);

            PuzzleVerification run = PuzzleVerifier.Verify(variant, solution, sim);
            Assert.GreaterOrEqual(run.ShipTime, variant.tmin - 1e-3f);
            Assert.LessOrEqual(run.ShipTime, variant.tmax + 1e-3f);
            Object.DestroyImmediate(variant);
        }


        [Test]
        public void Bake_FixedAtTheBakedZone()
        {
            PuzzleData variant = Bake(out PuzzleZone[] solution);

            Assert.IsFalse(variant.translatable);
            Assert.AreEqual(1, variant.moves);
            Assert.Less(Vector3.Distance(variant.zoneCentre, solution[0].centre), 1e-4f);
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
            authored.startMargin = 1.05f;
            authored.applyRandomAngle = true;
            authored.offPath = true;
            authored.windowTightness = 0.25f;

            PuzzleBrief mapped = PuzzleBrief.From(authored, settings);

            Assert.AreEqual(settings.cubeSizeMin, mapped.MinCubeSize, 1e-4f);
            Assert.AreEqual(settings.cubeSizeMax, mapped.MaxCubeSize, 1e-4f);
            Assert.AreEqual(settings.corridorClearance, mapped.CorridorClearance, 1e-4f);
            Assert.IsTrue(mapped.Translatable);
            Assert.AreEqual(2, mapped.Moves);
            Assert.IsTrue(mapped.Resizable);
            Assert.IsTrue(mapped.Rotate);
            Assert.AreEqual(1.05f, mapped.StartMargin, 1e-4f);
            Assert.IsTrue(mapped.ApplyRandomAngle);
            Assert.AreEqual(authored.seed, mapped.Seed);
            Assert.IsTrue(mapped.OffPath);
            Assert.AreEqual(0.25f, mapped.WindowTightness, 1e-4f);
            Object.DestroyImmediate(authored);
        }

        [Test]
        public void Bake_RejectsRotate()
        {
            PuzzleBrief rotating = brief;
            rotating.Rotate = true;

            PuzzleData rejected = PuzzleFieldBake.Bake(
                planets, sim, rotating, out PuzzleBakeStatus status, out _);

            Assert.IsNull(rejected);
            Assert.AreEqual(PuzzleBakeStatus.UnsupportedBrief, status);
        }

        [Test]
        public void Apply_UsesDeterministicSeededRandomAngle()
        {
            PuzzleBrief angled = brief;
            angled.ApplyRandomAngle = true;
            angled.Seed = KnownSeed;
            PuzzleData first = ScriptableObject.CreateInstance<PuzzleData>();
            PuzzleData second = ScriptableObject.CreateInstance<PuzzleData>();

            PuzzleFieldBake.Apply(angled, first, planets, sim);
            PuzzleFieldBake.Apply(angled, second, planets, sim);

            Assert.AreEqual(first.launchAngleDegs, second.launchAngleDegs, 1e-4f);
            Assert.GreaterOrEqual(Mathf.Abs(first.launchAngleDegs), 6f);
            Assert.LessOrEqual(Mathf.Abs(first.launchAngleDegs), 12f);
            Assert.AreEqual(first.launchAngleDegs, SpaceShapingSetup.Build(first, sim).LaunchAngleDegs, 1e-4f);
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }

        [Test]
        public void Apply_DisablesRandomAngleWhenSeedToggleIsOff()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.launchAngleDegs = 9f;

            PuzzleFieldBake.Apply(brief, puzzle, planets, sim);

            Assert.AreEqual(0f, puzzle.launchAngleDegs, 1e-4f);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Bake_RejectsFixedMultiZone()
        {
            PuzzleBrief multi = brief;
            multi.Moves = 2;

            PuzzleData rejected = PuzzleFieldBake.Bake(
                planets, sim, multi, out PuzzleBakeStatus status, out _);

            Assert.IsNull(rejected);
            Assert.AreEqual(PuzzleBakeStatus.UnsupportedBrief, status);
        }

        [Test]
        public void Bake_TranslatableMovesTheZoneOffHome()
        {
            PuzzleBrief movable = brief;
            movable.Translatable = true;

            PuzzleData baked = PuzzleFieldBake.Bake(
                planets, sim, movable, out PuzzleBakeStatus status, out PuzzleZone[] solution);

            Assert.AreEqual(PuzzleBakeStatus.Baked, status);
            Assert.IsTrue(baked.translatable);
            Vector3 home = (baked.origin + baked.destination) * 0.5f;
            Assert.Greater(Vector3.Distance(home, solution[0].centre), 1e-4f);
            Object.DestroyImmediate(baked);
        }

        private static PuzzleData Bake(out PuzzleZone[] solution)
        {
            PuzzleData variant = PuzzleFieldBake.Bake(
                planets, sim, brief, out PuzzleBakeStatus status, out solution);

            Assert.AreEqual(PuzzleBakeStatus.Baked, status);
            Assert.IsNotNull(variant);
            return variant;
        }


        private static void AssertVariant(PuzzleData variant, PuzzleZone[] solution)
        {
            PuzzleVerification check = PuzzleVerifier.Verify(variant, solution, sim);

            Assert.IsTrue(check.StartFails, $"start outcome {check.StartOutcome}");
            Assert.IsTrue(check.Solved, $"solved outcome {check.SolvedOutcome}");
            Assert.Greater(check.StartMargin, 0f);
            Assert.Greater(variant.tmax, variant.tmin);
            Assert.Greater(variant.entropyBudget, 0f);
            Assert.IsNotNull(solution);
            Assert.AreEqual(1, solution.Length);
            Assert.IsTrue(ContainsSource(variant, solution[0]));
            Assert.GreaterOrEqual(variant.cubeSize, variant.cubeSizeMin - 1e-4f);
            Assert.LessOrEqual(variant.cubeSize, variant.cubeSizeMax + 1e-4f);
            Assert.LessOrEqual(check.EntropySpent, variant.entropyBudget / 3f + 1e-3f);
        }

        private static bool ContainsSource(PuzzleData variant, PuzzleZone zone)
        {
            for (int i = 0; i < variant.sources.Length; i++)
            {
                Vector3 offset = variant.sources[i].home - zone.centre;
                if (Mathf.Abs(offset.x) <= zone.halfExtents.x
                    && Mathf.Abs(offset.y) <= zone.halfExtents.y
                    && Mathf.Abs(offset.z) <= zone.halfExtents.z)
                    return true;
            }

            return false;
        }

        private static PuzzlePlanetListAsset Planets()
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
