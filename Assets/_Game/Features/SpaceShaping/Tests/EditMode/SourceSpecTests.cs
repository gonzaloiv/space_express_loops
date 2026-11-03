using DigitalLove.Game.SpaceShaping.Cube;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class SourceSpecTests
    {
        [Test]
        public void Scaled_KeepsTheSourceName()
        {
            SourceSpec spec = new()
            {
                Home = new Vector3(1f, 0f, 0f),
                InitialScale = 2f,
                MinScale = 1f,
                MaxScale = 4f,
                Name = "solmuigawa"
            };

            SourceSpec scaled = spec.Scaled(Vector3.zero, 2f);

            Assert.AreEqual("solmuigawa", scaled.Name);
        }

        [Test]
        public void From_CarriesTheSourceName()
        {
            PuzzleSource[] sources =
            {
                new() { home = Vector3.zero, scale = 2f, name = "xanzonides" }
            };

            SourceSpec[] specs = SourceSpecs.From(sources, new PuzzleSimSettings());

            Assert.AreEqual("xanzonides", specs[0].Name);
        }

        [Test]
        public void From_KeepsAFixedMassAtItsInitialScale()
        {
            PuzzleSource[] sources =
            {
                new() { home = Vector3.zero, scale = 2f, fixedMass = true }
            };
            PuzzleSimSettings settings = new() { MinScaleFactor = 0.5f, MaxScaleFactor = 2f };

            SourceSpec[] specs = SourceSpecs.From(sources, settings);

            Assert.AreEqual(2f, specs[0].MinScale, 1e-4f);
            Assert.AreEqual(2f, specs[0].MaxScale, 1e-4f);
        }

        [Test]
        public void From_CarriesCentreOnly()
        {
            PuzzleSource[] sources =
            {
                new() { home = Vector3.zero, scale = 2f, centreOnly = true }
            };

            SourceSpec[] specs = SourceSpecs.From(sources, new PuzzleSimSettings());

            Assert.IsTrue(specs[0].CentreOnly);
        }

        [Test]
        public void From_CarriesFixedMass()
        {
            PuzzleSource[] sources =
            {
                new() { home = Vector3.zero, scale = 2f, fixedMass = true }
            };

            SourceSpec[] specs = SourceSpecs.From(sources, new PuzzleSimSettings());

            Assert.IsTrue(specs[0].FixedMass);
        }

        [Test]
        public void ApplyTo_KeepsAFixedSourceInPlaceAndSize()
        {
            Vector3[] positions = { Vector3.right, -Vector3.right };
            float[] scales = { 2f, 2f };
            SourceSpec[] specs = { Fixed(Vector3.right, 2f), Free(-Vector3.right, 2f) };
            SpaceZone zone = new(Vector3.zero, Vector3.one * 2f, 2f, 0f);

            zone.ApplyTo(positions, scales, specs);

            Assert.AreEqual(Vector3.right, positions[0]);
            Assert.AreEqual(2f, scales[0], 1e-4f);
            Assert.AreEqual(-Vector3.right * 2f, positions[1]);
            Assert.AreEqual(4f, scales[1], 1e-4f);
        }

        [Test]
        public void Apply_DoesNotMoveAFixedSource()
        {
            Vector3 other = new(0.05f, 0f, 0f);
            Vector3[] positions = { Vector3.zero, other };
            float[] scales = { 0.1f, 0.1f };
            SourceSpec[] specs = { Fixed(Vector3.zero, 0.1f), Free(other, 0.1f) };

            SourceConstraints.Apply(positions, scales, specs, 1.2f);

            Assert.AreEqual(Vector3.zero, positions[0]);
            Assert.AreEqual(0.12f, positions[1].x, 1e-4f);
        }

        private static SourceSpec Fixed(Vector3 home, float scale) =>
            new() { Home = home, InitialScale = scale, MinScale = scale, MaxScale = scale, FixedMass = true };

        private static SourceSpec Free(Vector3 home, float scale) =>
            new() { Home = home, InitialScale = scale, MinScale = scale * 0.5f, MaxScale = scale * 2f };
    }
}
