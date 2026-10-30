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
            seed.zones = 2;
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
        public void Brief_SupportsFixedAndMovableMultiZone()
        {
            Assert.IsTrue(brief.SupportsZones);

            PuzzleBrief fixedMulti = brief;
            fixedMulti.Translatable = false;
            Assert.IsTrue(fixedMulti.SupportsZones);

            PuzzleBrief tooMany = brief;
            tooMany.Zones = 4;
            Assert.IsFalse(tooMany.SupportsZones);
        }

        [Test]
        public void Search_FixedMatchesMovable()
        {
            PuzzleData field = WideField(brief);
            PuzzleBrief fixedTwo = brief;
            fixedTwo.Translatable = false;

            bool movableFound = PuzzleSolutionSearch.TrySelect(field, sim, brief, out PuzzleSolution movable);
            bool fixedFound = PuzzleSolutionSearch.TrySelect(field, sim, fixedTwo, out PuzzleSolution fixedZones);

            Assert.AreEqual(movableFound, fixedFound);
            if (!movableFound)
            {
                Object.DestroyImmediate(field);
                return;
            }

            Assert.AreEqual(movable.Zones.Length, fixedZones.Zones.Length);
            for (int i = 0; i < movable.Zones.Length; i++)
                Assert.Less(Vector3.Distance(movable.Zones[i].centre, fixedZones.Zones[i].centre), 1e-4f);
            Object.DestroyImmediate(field);
        }

        [Test]
        public void Bake_RejectsASeedWithAShorterZone()
        {
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = 1;
            seed.translatable = true;
            seed.zones = 2;
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
        public void Search_ThreeZones()
        {
            PuzzleBrief three = brief;
            three.Zones = 3;
            PuzzleData field = WideField(three);

            Assert.IsTrue(PuzzleSolutionSearch.TrySelect(field, sim, three, out PuzzleSolution first));
            Assert.AreEqual(3, first.Zones.Length);
            AssertZones(field, first.Zones);
            AssertSharedSize(first.Zones);

            Assert.IsTrue(PuzzleSolutionSearch.TrySelect(field, sim, three, out PuzzleSolution second));
            Assert.AreEqual(first.Zones.Length, second.Zones.Length);
            Object.DestroyImmediate(field);
        }

        [Test]
        public void ZoneRun_ArrayOverloadMatchesTheWrapper()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = new Vector3(-0.5f, 0f, 0f);
            puzzle.destination = new Vector3(0.5f, 0f, 0f);
            puzzle.cubeSizeMin = 0.1f;
            puzzle.cubeSizeMax = 0.5f;
            puzzle.zones = 2;
            puzzle.sources = new[] { new PuzzleSource { home = new Vector3(0f, 0.2f, 0f), scale = 0.1f } };
            PuzzleZone zone = PuzzleZoneCandidates.Zone(Vector3.zero, 0.2f, 1.4f, settings.zoneFalloff);

            PuzzleZoneRun.TryArrive(puzzle, sim, zone, out PuzzleSolution single);
            PuzzleZoneRun.TryArrive(puzzle, sim, new[] { zone }, out PuzzleSolution array);

            Assert.AreEqual(single.Zones.Length, array.Zones.Length);
            Assert.AreEqual(single.EntropySpent, array.EntropySpent, 1e-5f);
            Object.DestroyImmediate(puzzle);
        }

        private PuzzleData WideField(PuzzleBrief value)
        {
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            PuzzleFieldBake.Apply(value, field, planets, sim);
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
