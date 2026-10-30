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

            Assert.AreEqual(first.entropyBudget, second.entropyBudget, 1e-4f);
            Assert.Less(Vector3.Distance(first.zoneCentre, second.zoneCentre), 1e-4f);
            Assert.AreEqual(firstSolution[0].scale, secondSolution[0].scale, 1e-4f);
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }

        [Test]
        public void Bake_FixedAtTheBakedZone()
        {
            PuzzleData variant = Bake(out PuzzleZone[] solution);

            Assert.IsFalse(variant.translatable);
            Assert.AreEqual(1, variant.moves);
            Assert.AreEqual(1, variant.frames.Length);
            Assert.Less(Vector3.Distance(variant.frames[0].centre, solution[0].centre), 1e-4f);
            Object.DestroyImmediate(variant);
        }

        [Test]
        public void Brief_CarriesTheSeedConditions()
        {
            PuzzleSeed authored = ScriptableObject.CreateInstance<PuzzleSeed>();
            authored.translatable = true;
            authored.moves = 2;
            authored.resizable = true;

            PuzzleBrief mapped = PuzzleBrief.From(authored, settings);

            Assert.AreEqual(settings.endpointClearance, mapped.EndpointClearance, 1e-4f);
            Assert.IsTrue(mapped.Translatable);
            Assert.AreEqual(2, mapped.Moves);
            Assert.IsTrue(mapped.Resizable);
            Object.DestroyImmediate(authored);
        }

        [Test]
        public void Bake_FixedMultiZone_IsSearched()
        {
            PuzzleBrief multi = brief;
            multi.Moves = 2;

            PuzzleFieldBake.Bake(planets, sim, multi, out PuzzleBakeStatus status, out _);

            Assert.AreNotEqual(PuzzleBakeStatus.UnsupportedBrief, status);
        }

        [Test]
        public void Frames_FixedShipsOnePerZone()
        {
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            field.zoneCentre = new Vector3(1f, 2f, 3f);
            PuzzleBrief fixedBrief = brief;
            fixedBrief.Translatable = false;
            PuzzleSolution solution = new()
            {
                Zones = new[]
                {
                    PuzzleZoneCandidates.Zone(new Vector3(0.1f, 0f, 0f), 0.2f, 1.4f, 0.5f),
                    PuzzleZoneCandidates.Zone(new Vector3(-0.1f, 0f, 0f), 0.2f, 0.6f, 0.5f)
                }
            };

            PuzzleFrame[] frames = PuzzleFieldBake.BuildFrames(field, solution, fixedBrief);

            Assert.AreEqual(2, frames.Length);
            Assert.Less(Vector3.Distance(frames[0].centre, solution.Zones[0].centre), 1e-4f);
            Assert.AreEqual(0.2f, frames[0].size, 1e-4f);
            Assert.Less(Vector3.Distance(frames[1].centre, solution.Zones[1].centre), 1e-4f);
            Object.DestroyImmediate(field);
        }

        [Test]
        public void Frames_MovableShipsOneStart()
        {
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            field.zoneCentre = new Vector3(1f, 2f, 3f);
            PuzzleBrief movable = brief;
            movable.Translatable = true;
            PuzzleSolution solution = new()
            {
                Zones = new[] { PuzzleZoneCandidates.Zone(new Vector3(9f, 9f, 9f), 0.2f, 1.4f, 0.5f) }
            };

            PuzzleFrame[] frames = PuzzleFieldBake.BuildFrames(field, solution, movable);

            Assert.AreEqual(1, frames.Length);
            Assert.Less(Vector3.Distance(frames[0].centre, field.zoneCentre), 1e-4f);
            Assert.AreEqual(0.2f, frames[0].size, 1e-4f);
            Object.DestroyImmediate(field);
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
            Assert.Greater(variant.entropyBudget, 0f);
            Assert.IsNotNull(solution);
            Assert.AreEqual(1, solution.Length);
            Assert.IsTrue(ContainsSource(variant, solution[0]));
            Assert.AreEqual(1, variant.frames.Length);
            Assert.GreaterOrEqual(variant.frames[0].size, variant.cubeSizeMin - 1e-4f);
            Assert.LessOrEqual(variant.frames[0].size, variant.cubeSizeMax + 1e-4f);
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
