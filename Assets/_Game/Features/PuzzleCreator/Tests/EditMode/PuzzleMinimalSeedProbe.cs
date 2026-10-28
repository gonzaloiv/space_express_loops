using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleMinimalSeedProbe
    {
        [Test]
        public void Probe_MinimalTwoZoneSeed()
        {
            SpaceShapingSettings settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = 1;
            seed.translatable = true;
            seed.moves = 2;
            seed.resizable = false;

            PuzzleSeedScanResult result = PuzzleSeedScanner.Find(seed, settings, 24);
            int zones = result.Solution == null ? 0 : result.Solution.Length;
            Debug.Log($"MINIMAL found={result.Found} seed={result.Seed} status={result.Status} zones={zones}");

            Object.DestroyImmediate(seed);
            Object.DestroyImmediate(settings);
        }
    }
}
