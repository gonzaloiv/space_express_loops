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
            SectorFieldData field = PuzzleFieldBuilder.Build(seed, Settings());

            Assert.IsTrue(SectorField.FitsInCells(field));
            Assert.LessOrEqual(seed.distance, field.fieldRadius * 2f);
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
            SourceSpec[] specs = SourceSpecs.From(sources, settings);

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
        public void Cluster_FitsInsideTheRun()
        {
            PuzzleSeed seed = Seed();
            SpaceShapingSettings settings = Settings();
            PuzzleFieldPlanet[] planets = Planets(seed, settings);
            float half = seed.distance * 0.5f;

            for (int i = 0; i < planets.Length; i++)
                Assert.Less(planets[i].position.magnitude + planets[i].radius + settings.captureRadius, half);
        }

        [Test]
        public void Bake_FillsThePuzzleFromTheSeed()
        {
            PuzzleSeed seed = Seed();
            SpaceShapingSettings settings = Settings();
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();

            PuzzleFieldBake.Apply(puzzle, seed, settings);

            PuzzleFieldPlanet[] planets = Planets(seed, settings);
            Assert.AreEqual(planets.Length, puzzle.sources.Length);
            Assert.AreEqual(seed.distance, puzzle.distance, 1e-4f);

            for (int i = 0; i < planets.Length; i++)
            {
                Assert.AreEqual(planets[i].position, puzzle.sources[i].home);
                Assert.AreEqual(planets[i].radius * 2f, puzzle.sources[i].scale, 1e-4f);
            }
        }

        private static PuzzleSeed Seed() => ScriptableObject.CreateInstance<PuzzleSeed>();

        private static SpaceShapingSettings Settings() => ScriptableObject.CreateInstance<SpaceShapingSettings>();

        private static PuzzleFieldPlanet[] Planets(PuzzleSeed seed, SpaceShapingSettings settings) =>
            PuzzleFieldGenerator.Generate(PuzzleFieldBuilder.Build(seed, settings));
    }
}
