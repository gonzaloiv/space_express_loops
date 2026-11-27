using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    // Real probes, not fakes: two seeds of a small two-ship contract run through the batch with one
    // and two workers, and the answers must match. Bake-heavy: two full fleet probes per run.
    public class PuzzleSeedBatchRealTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";
        private const int Seeds = 2;

        [Test]
        public void Run_TwoRealSeeds_MatchBetweenOneAndTwoWorkers()
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            PuzzleJobContract job = PuzzleJobContracts.ById("02_02");
            PuzzleSeed seed = job.CreateSeed();
            try
            {
                PuzzleBrief brief = PuzzleBrief.From(seed, settings);
                List<PuzzleSeedScanResult> single = Run(job, settings, brief, seed, workers: 1);
                List<PuzzleSeedScanResult> paired = Run(job, settings, brief, seed, workers: 2);
                try
                {
                    AssertSame(single, paired);
                }
                finally
                {
                    Dispose(single);
                    Dispose(paired);
                }
            }
            finally
            {
                Object.DestroyImmediate(seed);
            }
        }

        private static List<PuzzleSeedScanResult> Run(
            PuzzleJobContract job, SpaceShapingSettings settings, PuzzleBrief brief, PuzzleSeed seed,
            int workers)
        {
            PuzzleSeedBatch batch = new(
                seedValue => PuzzleMultiShipSearch.PrepareForSeed(seed, settings, brief, seedValue),
                job.Seed, Seeds, workers);
            return batch.Run();
        }

        private static void AssertSame(
            List<PuzzleSeedScanResult> single, List<PuzzleSeedScanResult> paired)
        {
            Assert.AreEqual(single.Count, paired.Count);
            for (int i = 0; i < single.Count; i++)
            {
                PuzzleSeedScanResult expected = single[i];
                PuzzleSeedScanResult actual = paired[i];
                Assert.AreEqual(expected.Seed, actual.Seed);
                Assert.AreEqual(expected.Found, actual.Found);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.Kind, actual.Kind);
                if (expected.Found)
                    AssertZone(expected.Solution, actual.Solution);
            }
        }

        private static void AssertZone(PuzzleZone[] expected, PuzzleZone[] actual)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i].centre, actual[i].centre);
                Assert.AreEqual(expected[i].halfExtents, actual[i].halfExtents);
                Assert.AreEqual(expected[i].scale, actual[i].scale, 1e-6f);
                Assert.AreEqual(expected[i].falloff, actual[i].falloff, 1e-6f);
            }
        }

        private static void Dispose(List<PuzzleSeedScanResult> results)
        {
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Puzzle != null)
                    Object.DestroyImmediate(results[i].Puzzle);
            }
        }
    }
}
