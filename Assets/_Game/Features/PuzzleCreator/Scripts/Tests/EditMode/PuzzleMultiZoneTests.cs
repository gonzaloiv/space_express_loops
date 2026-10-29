using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleMultiZoneTests
    {
        private SpaceShapingSettings settings;
        private PuzzleSimSettings sim;
        private PuzzlePlanetListAsset planets;
        private PuzzleBrief brief;

        [OneTimeSetUp]
        public void SetUp()
        {
            settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            sim = PuzzleSimSettings.From(settings);
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = 2;
            seed.translatable = true;
            seed.moves = 2;
            seed.resizable = false;
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
        public void Brief_SupportsTranslatableMultiZone()
        {
            Assert.IsTrue(brief.SupportsMoves);

            PuzzleBrief fixedMulti = brief;
            fixedMulti.Translatable = false;
            Assert.IsFalse(fixedMulti.SupportsMoves);

            PuzzleBrief tooMany = brief;
            tooMany.Moves = 4;
            Assert.IsFalse(tooMany.SupportsMoves);

            PuzzleBrief offPath = brief;
            offPath.OffPath = true;
            Assert.IsFalse(offPath.SupportsMoves);
        }

        [Test]
        public void Bake_RejectsASeedWithAShorterZone()
        {
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = 1;
            seed.translatable = true;
            seed.moves = 2;
            PuzzleBrief shorter = PuzzleBrief.From(seed, settings);
            PuzzlePlanetListAsset field = Planets(seed);

            PuzzleData rejected = PuzzleFieldBake.Bake(
                field, sim, shorter, out PuzzleBakeStatus status, out _);

            Assert.IsNull(rejected);
            Assert.AreEqual(PuzzleBakeStatus.FewerZones, status);
            Object.DestroyImmediate(field);
            Object.DestroyImmediate(seed);
        }

        [Test]
        public void Zone_OverlapUsesTheSharedGapRule()
        {
            PuzzleZone a = PuzzleZoneCandidates.Zone(Vector3.zero, 0.2f, 1f, settings.zoneFalloff);
            PuzzleZone near = PuzzleZoneCandidates.Zone(
                new Vector3(0.05f, 0f, 0f), 0.2f, 1f, settings.zoneFalloff);
            PuzzleZone sameCentre = PuzzleZoneCandidates.Zone(Vector3.zero, 0.3f, 1f, settings.zoneFalloff);
            PuzzleZone far = PuzzleZoneCandidates.Zone(
                new Vector3(1f, 0f, 0f), 0.2f, 1f, settings.zoneFalloff);

            Assert.IsTrue(a.Overlaps(near, 0.25f));
            Assert.IsTrue(a.Overlaps(sameCentre, 0.25f));
            Assert.IsFalse(a.Overlaps(far, 0.25f));
        }

        [Test]
        public void Bake_ProducesTwoZones()
        {
            PuzzleData baked = Bake(out PuzzleZone[] solution);

            Assert.AreEqual(2, solution.Length);
            Assert.AreEqual(2, baked.moves);
            Assert.IsTrue(baked.translatable);
            Assert.IsTrue(ContainsSource(baked, solution[0]));
            Assert.IsTrue(ContainsSource(baked, solution[1]));
            Assert.IsFalse(Overlap(solution[0], solution[1]));
            Assert.AreEqual(solution[0].halfExtents.x, solution[1].halfExtents.x, 1e-4f);

            PuzzleVerification check = PuzzleVerifier.Verify(baked, solution, sim);
            Assert.IsTrue(check.StartFails);
            Assert.IsTrue(check.Solved);
            Assert.LessOrEqual(check.EntropySpent, baked.entropyBudget / 3f + 1e-3f);
            Object.DestroyImmediate(baked);
        }

        [Test]
        public void Bake_IsDeterministic()
        {
            PuzzleData first = Bake(out PuzzleZone[] firstSolution);
            PuzzleData second = Bake(out PuzzleZone[] secondSolution);

            Assert.AreEqual(firstSolution.Length, secondSolution.Length);
            Assert.AreEqual(firstSolution[0].centre, secondSolution[0].centre);
            Assert.AreEqual(firstSolution[1].centre, secondSolution[1].centre);
            Assert.AreEqual(first.moves, second.moves);
            Assert.AreEqual(first.entropyBudget, second.entropyBudget, 1e-4f);
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }

        [Test]
        public void Baked_FrameStartsAtTheRunMidpoint()
        {
            PuzzleData baked = Bake(out _);

            Assert.Less(Vector3.Distance(baked.zoneCentre, (baked.origin + baked.destination) * 0.5f), 1e-4f);
            Object.DestroyImmediate(baked);
        }

        [Test]
        public void Search_ThreeZonesOnAWideWindow()
        {
            PuzzleBrief three = brief;
            three.Moves = 3;
            PuzzleData field = WideField(three);

            Assert.IsTrue(PuzzleSolutionSearch.TrySelect(field, sim, three, out PuzzleSolution first));
            Assert.AreEqual(3, first.Zones.Length);
            AssertZones(field, first.Zones);
            AssertSharedSize(first.Zones);

            Assert.IsTrue(PuzzleSolutionSearch.TrySelect(field, sim, three, out PuzzleSolution second));
            Assert.AreEqual(first.ShipTime, second.ShipTime, 1e-4f);
            Object.DestroyImmediate(field);
        }

        [Test]
        public void ZoneRun_ArrayOverloadMatchesTheWrapper()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = new Vector3(-0.5f, 0f, 0f);
            puzzle.destination = new Vector3(0.5f, 0f, 0f);
            puzzle.launchSpeed = 1f;
            puzzle.tmin = 0f;
            puzzle.tmax = 10f;
            puzzle.cubeSizeMin = settings.cubeSizeMin;
            puzzle.cubeSizeMax = settings.cubeSizeMax;
            puzzle.moves = 2;
            puzzle.sources = new[] { new PuzzleSource { home = new Vector3(0f, 0.2f, 0f), scale = 0.1f } };
            PuzzleZone zone = PuzzleZoneCandidates.Zone(Vector3.zero, 0.2f, 1.4f, settings.zoneFalloff);

            PuzzleZoneRun.TryArrive(puzzle, sim, zone, out PuzzleSolution single);
            PuzzleZoneRun.TryArrive(puzzle, sim, new[] { zone }, out PuzzleSolution array);

            Assert.AreEqual(single.ShipTime, array.ShipTime, 1e-5f);
            Assert.AreEqual(single.EntropySpent, array.EntropySpent, 1e-5f);
            Object.DestroyImmediate(puzzle);
        }

        private PuzzleData Bake(out PuzzleZone[] solution)
        {
            PuzzleData variant = PuzzleFieldBake.Bake(
                planets, sim, brief, out PuzzleBakeStatus status, out solution);

            Assert.AreEqual(PuzzleBakeStatus.Baked, status);
            Assert.IsNotNull(variant);
            return variant;
        }

        private PuzzleData WideField(PuzzleBrief value)
        {
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            PuzzleFieldBake.Apply(value, field, planets, sim);
            field.tmin = 0f;
            field.tmax = field.Distance / settings.nominalSpeed * 2f;
            return field;
        }

        private static void AssertZones(PuzzleData puzzle, PuzzleZone[] zones)
        {
            for (int i = 0; i < zones.Length; i++)
            {
                Assert.IsTrue(ContainsSource(puzzle, zones[i]));
                for (int j = i + 1; j < zones.Length; j++)
                    Assert.IsFalse(Overlap(zones[i], zones[j]));
            }
        }

        private static void AssertSharedSize(PuzzleZone[] zones)
        {
            for (int i = 1; i < zones.Length; i++)
                Assert.AreEqual(zones[0].halfExtents.x, zones[i].halfExtents.x, 1e-4f);
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

        private static bool Overlap(PuzzleZone a, PuzzleZone b)
        {
            Vector3 delta = a.centre - b.centre;
            Vector3 extent = a.halfExtents + b.halfExtents;
            return Mathf.Abs(delta.x) < extent.x
                && Mathf.Abs(delta.y) < extent.y
                && Mathf.Abs(delta.z) < extent.z;
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
