using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleShipScanTests
    {
        private const int KnownSeed = 2;

        private SpaceShapingSettings settings;
        private PuzzleSimSettings sim;
        private PuzzleBrief brief;
        private PuzzlePlanetListAsset planets;

        [OneTimeSetUp]
        public void SetUp()
        {
            settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            sim = PuzzleSimSettings.From(settings);
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = KnownSeed;
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
        public void Rescued_NeutralZone_FindsNothing()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = new Vector3(-0.5f, 0f, 0f);
            puzzle.destination = new Vector3(0.5f, 0f, 0f);
            puzzle.sources = new[]
            {
                new PuzzleSource { home = new Vector3(-0.5f, 0f, 0f), scale = 0.1f },
                new PuzzleSource { home = new Vector3(0.5f, 0f, 0f), scale = 0.1f }
            };
            PuzzleZone neutral = PuzzleZoneCandidates.Zone(
                new Vector3(10f, 10f, 10f), 0.2f, 1f, sim.ZoneFalloff);

            List<PuzzleShipRescue> rescued = new PuzzleShipScan(puzzle, sim, 0f).Rescued(neutral);

            Assert.IsEmpty(rescued);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Rescued_FindsTheBakedSolutionRun()
        {
            PuzzleData field = Field();
            Assert.IsTrue(PuzzleSolutionSearch.TrySelect(field, sim, brief, out PuzzleSolution solution));
            PuzzleZone zone = solution.Zones[0];

            List<PuzzleShipRescue> rescued = new PuzzleShipScan(field, sim, 0f).Rescued(zone);

            Assert.IsTrue(
                Has(rescued, field.origin, field.destination),
                "the solved zone did not rescue its own run");
            Object.DestroyImmediate(field);
        }

        [Test]
        public void Rescued_SkipsPairsTheZoneTouches()
        {
            PuzzleData field = Field();
            Assert.IsTrue(PuzzleSolutionSearch.TrySelect(field, sim, brief, out PuzzleSolution solution));
            PuzzleZone zone = solution.Zones[0];

            List<PuzzleShipRescue> rescued = new PuzzleShipScan(field, sim, 0f).Rescued(zone);

            for (int i = 0; i < rescued.Count; i++)
            {
                Assert.IsFalse(zone.Contains(rescued[i].Origin));
                Assert.IsFalse(zone.Contains(rescued[i].Destination));
                Assert.GreaterOrEqual(rescued[i].Margin, 0f);
            }

            Object.DestroyImmediate(field);
        }

        [Test]
        public void Rescued_NeverStartsOrEndsAtTheSun()
        {
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = KnownSeed;
            seed.hasSun = true;
            float sunRadius = settings.sunRadius;
            settings.sunRadius = 0.06f; // the project's value; the test settings default to no sun
            PuzzleBrief sunBrief = PuzzleBrief.From(seed, settings);
            PuzzlePlanetListAsset sunPlanets = Planets(seed);
            settings.sunRadius = sunRadius;
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            PuzzleFieldBake.Apply(sunBrief, field, sunPlanets, sim);
            Vector3? sun = SunOf(field);
            Assert.IsTrue(sun.HasValue, "the test field has no sun");
            if (!PuzzleSolutionSearch.TrySelect(field, sim, sunBrief, out PuzzleSolution solution))
                Assert.Inconclusive("no solution zone on the sun field to scan with");

            PuzzleShipScan scan = new(field, sim, 0f);
            AssertNoSun(scan.Rescued(solution.Zones[0]), sun.Value);
            AssertNoSun(scan.RescuedFixed(solution.Zones[0]), sun.Value);
            Object.DestroyImmediate(field);
            Object.DestroyImmediate(sunPlanets);
            Object.DestroyImmediate(seed);
        }

        [Test]
        public void RescuedFixed_NeverExceedsRuntime()
        {
            PuzzleData field = Field();
            Assert.IsTrue(PuzzleSolutionSearch.TrySelect(field, sim, brief, out PuzzleSolution solution));
            PuzzleZone zone = solution.Zones[0];
            PuzzleShipScan scan = new(field, sim, 0f);

            Assert.LessOrEqual(scan.RescuedFixed(zone).Count, scan.Rescued(zone).Count);
            Object.DestroyImmediate(field);
        }

        [Test]
        public void Rescued_IsDeterministic()
        {
            PuzzleData field = Field();
            Assert.IsTrue(PuzzleSolutionSearch.TrySelect(field, sim, brief, out PuzzleSolution solution));
            PuzzleZone zone = solution.Zones[0];
            PuzzleShipScan scan = new(field, sim, 0f);

            List<PuzzleShipRescue> first = scan.Rescued(zone);
            List<PuzzleShipRescue> second = scan.Rescued(zone);

            Assert.AreEqual(first.Count, second.Count);
            for (int i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first[i].Origin, second[i].Origin);
                Assert.AreEqual(first[i].Destination, second[i].Destination);
                Assert.AreEqual(first[i].Margin, second[i].Margin, 1e-6f);
            }

            Object.DestroyImmediate(field);
        }

        private static bool Has(List<PuzzleShipRescue> rescued, Vector3 origin, Vector3 destination)
        {
            for (int i = 0; i < rescued.Count; i++)
            {
                if ((rescued[i].Origin - origin).sqrMagnitude < 1e-8f
                    && (rescued[i].Destination - destination).sqrMagnitude < 1e-8f)
                    return true;
            }

            return false;
        }

        private static Vector3? SunOf(PuzzleData field)
        {
            for (int i = 0; i < field.sources.Length; i++)
            {
                if (field.sources[i].centreOnly)
                    return field.sources[i].home;
            }

            return null;
        }

        private static void AssertNoSun(List<PuzzleShipRescue> rescued, Vector3 sun)
        {
            for (int i = 0; i < rescued.Count; i++)
            {
                Assert.Greater((rescued[i].Origin - sun).sqrMagnitude, 1e-8f, "a ship starts at the sun");
                Assert.Greater((rescued[i].Destination - sun).sqrMagnitude, 1e-8f, "a ship ends at the sun");
            }
        }

        private PuzzleData Field()
        {
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            PuzzleFieldBake.Apply(brief, field, planets, sim);
            return field;
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
