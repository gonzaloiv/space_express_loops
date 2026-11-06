using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    // Bakes a job's contract from Seed to Seed + count - 1 and measures each baked seed's solve window,
    // so the most forgiving seed can be pinned in the contract. Nothing is saved.
    public static class PuzzleJobSeedScan
    {
        public struct Entry
        {
            public int Seed;
            public bool Baked;
            public float Window;
            public PuzzleSolutionKind Kind;
            public PuzzleBakeStatus Status;
        }

        public static List<Entry> Scan(PuzzleJobContract job, int count)
        {
            List<Entry> entries = new();
            SpaceShapingSettings settings = PuzzleTestSetBuilder.LoadSettings("Seed scan");
            if (settings == null)
                return entries;

            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleSeed seed = PuzzleTestSetBuilder.CreateSeed(job);
            try
            {
                for (int i = 0; i < count; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(
                            "Seed scan", $"{job.Id}: seed {job.Seed + i}", i / (float)count))
                        break;

                    seed.seed = job.Seed + i;
                    entries.Add(Probe(seed, settings, sim));
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                Object.DestroyImmediate(seed);
            }

            entries.Sort((a, b) => b.Window.CompareTo(a.Window));
            return entries;
        }

        private static Entry Probe(PuzzleSeed seed, SpaceShapingSettings settings, PuzzleSimSettings sim)
        {
            PuzzleSeedScanResult result = PuzzleSeedScanner.Find(seed, settings, 1);
            Entry entry = new()
            {
                Seed = seed.seed,
                Baked = result.Found,
                Kind = result.Kind,
                Status = result.Status
            };

            if (result.Found)
                entry.Window = PuzzleSolveWindow.Measure(result.Puzzle, result.Solution, sim);

            if (result.Puzzle != null)
                Object.DestroyImmediate(result.Puzzle);

            return entry;
        }
    }
}
