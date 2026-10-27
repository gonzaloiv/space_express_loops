using DigitalLove.Game.Sectors;
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
            Assert.Less(shape.radius.max, settings.runDistance);
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
        public void Build_SpacesWiderThanThePuzzleNeeds()
        {
            SpaceShapingSettings settings = Settings();
            SectorShapeData shape = PuzzleFieldBuilder.Build(Seed(), settings);

            Assert.GreaterOrEqual(shape.minSeparation, settings.separationScale * 1.1f);
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
        public void ShapeAsset_CarriesSeedAndDistance()
        {
            PuzzleSeed seed = Seed();
            PuzzleShapeAsset asset = PuzzleArtifactFactory.Shape(seed, Settings());

            Assert.AreEqual(seed.seed, asset.seed);
            Assert.AreEqual(seed.distance, asset.distance, 1e-4f);
        }

        [Test]
        public void PlanetList_CarriesDistance()
        {
            PuzzleSeed seed = Seed();
            PuzzlePlanetListAsset asset = PuzzleArtifactFactory.Planets(
                PuzzleArtifactFactory.Shape(seed, Settings()));

            Assert.AreEqual(seed.distance, asset.distance, 1e-4f);
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
                PuzzleBrief.From(seed), puzzle, planets, PuzzleSimSettings.From(settings));

            Assert.AreEqual(planets.planets.Length, puzzle.sources.Length);
            Assert.Greater(puzzle.Distance, settings.runDistance * 0.5f);
            Assert.Less(puzzle.Distance, settings.runDistance * 1.5f);
            Assert.Less(Vector3.Distance((puzzle.origin + puzzle.destination) * 0.5f, puzzle.zoneCentre), 1e-4f);
            Assert.Greater(puzzle.launchSpeed, 0f);
        }

        [Test]
        public void BuildSetup_PrefersThePuzzleLaunch()
        {
            PuzzleData puzzle = Bare();
            puzzle.launchSpeed = 2f;

            SpaceShapingSetup setup = SpaceShapingSetup.Build(puzzle, PuzzleSimSettings.From(Settings()));

            Assert.AreEqual(2f, setup.LaunchSpeed, 1e-4f);
        }

        private static PuzzleData Bare()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.sources = System.Array.Empty<PuzzleSource>();
            return puzzle;
        }

        private static PuzzleSeed Seed() => ScriptableObject.CreateInstance<PuzzleSeed>();

        private static SpaceShapingSettings Settings() => ScriptableObject.CreateInstance<SpaceShapingSettings>();

        private static PuzzleFieldPlanet[] Planets(PuzzleSeed seed, SpaceShapingSettings settings) =>
            PuzzleFieldGenerator.Generate(PuzzleFieldBuilder.Build(seed, settings), seed.seed);
    }
}
