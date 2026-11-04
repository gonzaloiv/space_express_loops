using DigitalLove.Game.Sectors;
using DigitalLove.Game.SpaceShaping.Cube;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleFieldTests
    {
        [Test]
        public void Generate_SameSeed_IsDeterministic()
        {
            PuzzleSeed seed = Seed();
            SpaceShapingSettings settings = Settings();
            PuzzleFieldPlanet[] first = Planets(seed, settings);
            PuzzleFieldPlanet[] second = Planets(seed, settings);

            Assert.AreEqual(first.Length, second.Length);
            for (int i = 0; i < first.Length; i++)
            {
                Assert.AreEqual(first[i].position, second[i].position);
                Assert.AreEqual(first[i].radius, second[i].radius);
            }
        }

        [Test]
        public void Generate_ProducesRequestedPlanetCount()
        {
            PuzzleSeed seed = Seed();
            Assert.AreEqual(seed.planetCount, Planets(seed, Settings()).Length);
        }

        [Test]
        public void Build_FieldFitsThePlaySpace()
        {
            PuzzleSeed seed = Seed();
            SpaceShapingSettings settings = Settings();
            SectorShapeData shape = PuzzleFieldBuilder.Build(seed, settings);

            Assert.AreEqual(seed.planetCount, shape.planetCount.max);
            float extent = shape.radius.max + shape.planetRadius.max;
            Assert.Greater(extent, settings.runDistance * 0.5f);
            Assert.Less(extent, settings.runDistance);
        }

        [Test]
        public void Build_SpansTheRun()
        {
            PuzzleSeed seed = Seed();
            SpaceShapingSettings settings = Settings();
            PuzzleFieldPlanet[] planets = Planets(seed, settings);
            float outer = 0f;
            for (int i = 0; i < planets.Length; i++)
                outer = Mathf.Max(outer, planets[i].position.magnitude + planets[i].radius);

            Assert.Greater(outer, settings.runDistance * 0.5f);
            Assert.Less(outer, settings.runDistance * 0.75f);
        }

        [Test]
        public void Build_CompactsTheThreePlanetField()
        {
            SpaceShapingSettings settings = Settings();
            PuzzleSeed few = Seed();
            few.planetCount = 3;
            PuzzleSeed many = Seed();
            many.planetCount = 6;

            Assert.AreEqual(0.6f, PuzzleFieldBuilder.Build(few, settings).spread, 1e-4f);
            Assert.AreEqual(0.7f, PuzzleFieldBuilder.Build(many, settings).spread, 1e-4f);
        }

        [Test]
        public void Build_ThreePlanetFieldStillSpansTheRun()
        {
            SpaceShapingSettings settings = Settings();
            PuzzleSeed seed = Seed();
            seed.planetCount = 3;

            PuzzleFieldPlanet[] planets = Planets(seed, settings);
            float outer = 0f;
            for (int i = 0; i < planets.Length; i++)
                outer = Mathf.Max(outer, planets[i].position.magnitude + planets[i].radius);

            Assert.Greater(outer, settings.runDistance * 0.5f);
            Assert.Less(outer, settings.runDistance * 0.75f);
        }

        [Test]
        public void Build_SpacesWiderThanThePuzzleNeeds()
        {
            SpaceShapingSettings settings = Settings();
            SectorShapeData shape = PuzzleFieldBuilder.Build(Seed(), settings);

            Assert.GreaterOrEqual(shape.minSeparation, settings.separationScale * 1.1f);
        }

        [Test]
        public void Build_PinsTheAuthoredFieldShape()
        {
            PuzzleSeed seed = Seed();
            SpaceShapingSettings settings = Settings();

            seed.fieldShape = PuzzleFieldShape.Cluster;
            Assert.AreEqual(SectorShapeKind.Cluster, PuzzleFieldBuilder.Build(seed, settings).kind);

            seed.fieldShape = PuzzleFieldShape.Auto;
            Assert.AreEqual(SectorShapeKind.Spiral, PuzzleFieldBuilder.Build(seed, settings).kind);
        }

        [Test]
        public void Build_PassesTheDepthKnobs()
        {
            SpaceShapingSettings settings = Settings();
            settings.fieldThickness = 0.05f;
            settings.fieldTilt = 9f;
            settings.fieldBulge = 0.6f;

            SectorShapeData shape = PuzzleFieldBuilder.Build(Seed(), settings);

            Assert.AreEqual(0.05f, shape.inclination, 1e-4f);
            Assert.AreEqual(9f, shape.tilt, 1e-4f);
            Assert.AreEqual(0.6f, shape.bulge, 1e-4f);
        }

        [Test]
        public void Build_AlwaysAsksForAnInnerRing()
        {
            PuzzleSeed seed = Seed();
            seed.planetCount = 6;

            SectorShapeData shape = PuzzleFieldBuilder.Build(seed, Settings());

            Assert.GreaterOrEqual(shape.ringCount.min, 2);
        }

        [Test]
        public void Bridge_MapsRadiusToDoubleScale()
        {
            PuzzleFieldPlanet[] planets = { new(new Vector3(1f, 2f, 3f), 0.5f) };

            PuzzleSource[] sources = PuzzleSourceBridge.ToSources(planets);

            Assert.AreEqual(new Vector3(1f, 2f, 3f), sources[0].home);
            Assert.AreEqual(1f, sources[0].scale, 1e-4f);
        }

        [Test]
        public void BakedSources_NeedNoCorrection()
        {
            PuzzleSeed seed = Seed();
            SpaceShapingSettings settings = Settings();
            PuzzleSource[] sources = PuzzleSourceBridge.ToSources(Planets(seed, settings));
            SourceSpec[] specs = SourceSpecs.From(sources, PuzzleSimSettings.From(settings));

            Vector3[] positions = new Vector3[specs.Length];
            float[] scales = new float[specs.Length];
            for (int i = 0; i < specs.Length; i++)
            {
                positions[i] = specs[i].Home;
                scales[i] = specs[i].InitialScale;
            }

            SourceConstraints.Apply(positions, scales, specs, settings.separationScale);

            for (int i = 0; i < specs.Length; i++)
                Assert.Less((positions[i] - specs[i].Home).magnitude, 1e-3f);
        }

        [Test]
        public void ShapeAsset_CarriesSeed()
        {
            PuzzleSeed seed = Seed();
            PuzzleShapeAsset asset = PuzzleArtifactFactory.Shape(seed, Settings());

            Assert.AreEqual(seed.seed, asset.seed);
        }

        [Test]
        public void Apply_AddsAFixedSunAtTheCentre()
        {
            PuzzleData puzzle = Applied(new PuzzleBrief { SunRadius = 0.15f, SunScalable = false });

            Assert.AreEqual(4, puzzle.sources.Length);
            Assert.AreEqual(Vector3.zero, puzzle.sources[0].home);
            Assert.IsTrue(puzzle.sources[0].fixedMass);
            Assert.IsTrue(puzzle.sources[0].centreOnly);
            Assert.AreEqual(0.3f, puzzle.sources[0].scale, 1e-4f);
            Assert.IsFalse(string.IsNullOrEmpty(puzzle.sources[0].name));
            Assert.AreNotEqual("Sun", puzzle.sources[0].name);
            for (int i = 1; i < puzzle.sources.Length; i++)
                Assert.AreNotEqual(puzzle.sources[0].name, puzzle.sources[i].name);
            Assert.AreNotEqual(Vector3.zero, puzzle.origin);
            Assert.AreNotEqual(Vector3.zero, puzzle.destination);
        }

        [Test]
        public void Apply_AddsAScalableSunAtTheCentre()
        {
            PuzzleData puzzle = Applied(new PuzzleBrief { SunRadius = 0.15f, SunScalable = true });

            Assert.IsFalse(puzzle.sources[0].fixedMass);
            Assert.IsTrue(puzzle.sources[0].centreOnly);
        }

        [Test]
        public void Apply_SeedWithoutASun_BakesNoSun()
        {
            PuzzleSeed seed = Seed();
            seed.hasSun = false;
            SpaceShapingSettings settings = Settings();
            settings.sunRadius = 0.15f;

            PuzzleData puzzle = Applied(PuzzleBrief.From(seed, settings));

            Assert.AreEqual(3, puzzle.sources.Length);
        }

        [Test]
        public void Apply_SeedWithASun_BakesOneAtTheSettingsRadius()
        {
            PuzzleSeed seed = Seed();
            seed.hasSun = true;
            SpaceShapingSettings settings = Settings();
            settings.sunRadius = 0.08f;

            PuzzleData puzzle = Applied(PuzzleBrief.From(seed, settings));

            Assert.AreEqual(4, puzzle.sources.Length);
            Assert.AreEqual(0.16f, puzzle.sources[0].scale, 1e-4f);
            Assert.IsFalse(puzzle.sources[0].fixedMass);
        }

        [Test]
        public void Brief_DefaultsToAScalableSun()
        {
            PuzzleSeed seed = Seed();
            seed.hasSun = true;
            SpaceShapingSettings settings = Settings();
            settings.sunRadius = 0.1f;

            Assert.IsTrue(PuzzleBrief.From(seed, settings).SunScalable);
        }

        [Test]
        public void Build_SetsTheAnchorToTheSunRadius()
        {
            PuzzleSeed seed = Seed();
            seed.hasSun = true;
            SpaceShapingSettings settings = Settings();
            settings.sunRadius = 0.15f;

            SectorShapeData shape = PuzzleFieldBuilder.Build(seed, settings);

            Assert.AreEqual(0.15f, shape.anchorRadius, 1e-4f);
        }

        [Test]
        public void Build_WithoutASun_LeavesNoAnchorHole()
        {
            PuzzleSeed seed = Seed();
            seed.hasSun = false;
            SpaceShapingSettings settings = Settings();
            settings.sunRadius = 0.15f;

            Assert.AreEqual(0f, PuzzleFieldBuilder.Build(seed, settings).anchorRadius, 1e-4f);
        }

        [Test]
        public void CoversSource_IgnoresAFixedSun()
        {
            PuzzleData puzzle = Bare();
            puzzle.sources = new[]
            {
                new PuzzleSource { home = Vector3.zero, scale = 1f, fixedMass = true },
                new PuzzleSource { home = new Vector3(0.2f, 0f, 0f), scale = 0.1f }
            };
            PuzzleZone zone = new()
            {
                centre = new Vector3(0.1f, 0f, 0f),
                halfExtents = Vector3.one * 0.25f,
                scale = 1f,
                falloff = 0f
            };

            Assert.IsTrue(PuzzleZoneCandidates.CoversSource(zone, puzzle));
        }

        [Test]
        public void Centres_ExcludeAFixedSun()
        {
            PuzzleData puzzle = Bare();
            puzzle.sources = new[]
            {
                new PuzzleSource { home = Vector3.zero, scale = 1f, fixedMass = true },
                new PuzzleSource { home = new Vector3(0.2f, 0f, 0f), scale = 0.1f }
            };

            Vector3[] centres = PuzzleZoneCandidates.Centres(puzzle, 0.1f);

            Assert.AreEqual(1, centres.Length);
            Assert.AreEqual(new Vector3(0.2f, 0f, 0f), centres[0]);
        }

        [Test]
        public void CoversSource_ScalesAScalableSunByItsCentre()
        {
            PuzzleData puzzle = Bare();
            puzzle.sources = new[]
            {
                new PuzzleSource { home = Vector3.zero, scale = 1f, centreOnly = true },
                new PuzzleSource { home = new Vector3(0.2f, 0f, 0f), scale = 0.1f }
            };
            PuzzleZone zone = new()
            {
                centre = Vector3.zero,
                halfExtents = Vector3.one * 0.01f,
                scale = 1f,
                falloff = 0f
            };

            Assert.IsTrue(PuzzleZoneCandidates.CoversSource(zone, puzzle));
        }

        [Test]
        public void Centres_IncludeAScalableSun()
        {
            PuzzleData puzzle = Bare();
            puzzle.sources = new[]
            {
                new PuzzleSource { home = Vector3.zero, scale = 1f, centreOnly = true },
                new PuzzleSource { home = new Vector3(0.2f, 0f, 0f), scale = 0.1f }
            };

            Vector3[] centres = PuzzleZoneCandidates.Centres(puzzle, 0.1f);

            Assert.AreEqual(2, centres.Length);
            CollectionAssert.Contains(centres, Vector3.zero);
        }

        [Test]
        public void Bake_PicksTheRunFromTheField()
        {
            PuzzleSeed seed = Seed();
            SpaceShapingSettings settings = Settings();
            PuzzlePlanetListAsset planets = PuzzleArtifactFactory.Planets(
                PuzzleArtifactFactory.Shape(seed, settings));
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();

            PuzzleFieldBake.Apply(
                PuzzleBrief.From(seed, settings), puzzle, planets, PuzzleSimSettings.From(settings));

            Assert.AreEqual(planets.planets.Length, puzzle.sources.Length);
            Assert.Greater(puzzle.Distance, settings.runDistance * 0.5f);
            Assert.Less(puzzle.Distance, settings.runDistance * 1.5f);
            Assert.Less(Vector3.Distance((puzzle.origin + puzzle.destination) * 0.5f, puzzle.zoneCentre), 1e-4f);
        }

        [Test]
        public void BuildSetup_LaunchSpeedIsFactorTimesNominalSpeed()
        {
            PuzzleData puzzle = Bare();
            SpaceShapingSettings settings = Settings();
            settings.launchSpeedFactor = 1.25f;
            settings.nominalSpeed = 2f;

            SpaceShapingSetup setup = SpaceShapingSetup.Build(puzzle, PuzzleSimSettings.From(settings));

            Assert.AreEqual(2.5f, setup.LaunchSpeed, 1e-4f);
        }

        private static PuzzleData Bare()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.sources = System.Array.Empty<PuzzleSource>();
            return puzzle;
        }

        private static PuzzleData Applied(PuzzleBrief brief)
        {
            PuzzlePlanetListAsset planets = ScriptableObject.CreateInstance<PuzzlePlanetListAsset>();
            planets.planets = new[]
            {
                new PuzzleFieldPlanet(new Vector3(-0.2f, 0f, 0f), 0.02f, "a"),
                new PuzzleFieldPlanet(new Vector3(0.2f, 0f, 0f), 0.02f, "b"),
                new PuzzleFieldPlanet(new Vector3(0f, 0.02f, 0f), 0.02f, "c")
            };
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            PuzzleSimSettings settings = new() { RunDistance = 0.5f };
            brief.SunOnChord = true;
            Assert.IsTrue(PuzzleFieldBake.Apply(brief, puzzle, planets, settings));
            return puzzle;
        }

        private static PuzzleSeed Seed() => ScriptableObject.CreateInstance<PuzzleSeed>();

        private static SpaceShapingSettings Settings() => ScriptableObject.CreateInstance<SpaceShapingSettings>();

        private static PuzzleFieldPlanet[] Planets(PuzzleSeed seed, SpaceShapingSettings settings) =>
            PuzzleFieldGenerator.Generate(PuzzleFieldBuilder.Build(seed, settings), seed.seed);
    }
}
