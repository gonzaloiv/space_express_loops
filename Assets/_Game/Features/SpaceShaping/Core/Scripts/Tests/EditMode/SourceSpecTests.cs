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
    }
}
