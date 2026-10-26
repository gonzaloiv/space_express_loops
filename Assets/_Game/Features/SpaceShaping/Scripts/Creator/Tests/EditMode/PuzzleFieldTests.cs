using DigitalLove.Game.Sectors;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleFieldTests
    {
        private const float PuzzleSeparation = 1.2f;
        private const float MinScaleFactor = 0.4f;
        private const float MaxScaleFactor = 2.5f;

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

            SourceSpec[] specs = PuzzleSourceBridge.ToSources(planets, 2f, MinScaleFactor, MaxScaleFactor);

            Assert.AreEqual(new Vector3(1f, 2f, 3f), specs[0].Home);
            Assert.AreEqual(1f, specs[0].InitialScale, 1e-4f);
            Assert.AreEqual(2f, specs[0].MoveRange, 1e-4f);
            Assert.AreEqual(0.4f, specs[0].MinScale, 1e-4f);
            Assert.AreEqual(2.5f, specs[0].MaxScale, 1e-4f);
        }

        [Test]
        public void Bridge_Sources_NeedNoCorrection()
        {
            PuzzleSeed seed = Seed();
            SpaceShapingSettings settings = Settings();
            SourceSpec[] specs = PuzzleSourceBridge.ToSources(
                Planets(seed, settings), settings.sourceMoveRange, MinScaleFactor, MaxScaleFactor);

            Vector3[] positions = new Vector3[specs.Length];
            float[] scales = new float[specs.Length];
            for (int i = 0; i < specs.Length; i++)
            {
                positions[i] = specs[i].Home;
                scales[i] = specs[i].InitialScale;
            }

            SourceConstraints.Apply(positions, scales, specs, PuzzleSeparation);

            for (int i = 0; i < specs.Length; i++)
                Assert.Less((positions[i] - specs[i].Home).magnitude, 1e-3f);
        }

        [Test]
        public void Layout_KeepsEndpointsClearOfTheCluster()
        {
            PuzzleSeed seed = Seed();
            SpaceShapingSettings settings = Settings();
            PuzzleFieldPlanet[] planets = Planets(seed, settings);
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.distance = PuzzleFieldLayout.RunLength(planets, seed.distance, settings.captureRadius);

            for (int i = 0; i < planets.Length; i++)
            {
                float clearance = planets[i].radius + settings.captureRadius;
                Assert.Greater((planets[i].position - puzzle.Origin).magnitude, clearance);
                Assert.Greater((planets[i].position - puzzle.Destination).magnitude, clearance);
            }
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
            Assert.GreaterOrEqual(puzzle.distance, seed.distance);

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
