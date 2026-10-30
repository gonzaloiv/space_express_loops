using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleShipPlateauTests
    {
        private const int Seed = 1;
        private const int Fields = 2;
        private const int Samples = 5;

        [Test]
        [Explicit("Spike measurement, not a regression test: run by name when sweeping feasibility.")]
        public void Plateau_ReportsScaleWidth()
        {
            SpaceShapingSettings settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = Seed;
            seed.planetCount = 9;

            PuzzleShipPlateauReport report = PuzzleShipPlateau.Run(seed, settings, Fields, Samples);
            Debug.Log(report.ToString());

            Assert.Greater(report.Baked, 0);
            Object.DestroyImmediate(seed);
            Object.DestroyImmediate(settings);
        }
    }
}
