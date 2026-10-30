using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleShipSweepTests
    {
        private const int Seed = 1;
        private const int Fields = 10;

        [Test]
        [Explicit("Spike measurement, not a regression test: run by name when sweeping feasibility.")]
        public void Sweep_ReportsFeasibility()
        {
            SpaceShapingSettings settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = Seed;
            seed.planetCount = 9;

            PuzzleShipSweepReport report = PuzzleShipSweep.Run(seed, settings, Fields, 0f);
            Debug.Log(report.ToString());

            Assert.Greater(report.Baked, 0);
            Object.DestroyImmediate(seed);
            Object.DestroyImmediate(settings);
        }
    }
}
