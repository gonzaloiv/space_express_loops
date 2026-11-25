using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    // Bakes a job's contract from Seed to Seed + count - 1, checks each baked seed against the whole
    // contract (the same validator JobSet_MeetsTheContract runs), and measures its solve window, so a
    // seed that will pass can be pinned in the contract. Nothing is saved.
    public static class PuzzleJobSeedScan
    {
        public struct Entry
        {
            public int Seed;
            public bool Baked;
            public bool MeetsContract;
            public string Problem;
            public float Window;
            public PuzzleSolutionKind Kind;
            public PuzzleBakeStatus Status;
            public PuzzleRunReport RunReport;
        }

        public static List<Entry> Scan(PuzzleJobContract job, int count)
        {
            List<Entry> entries = new();
            SpaceShapingSettings settings = PuzzleTestSetBuilder.LoadSettings("Seed scan");
            if (settings == null)
                return entries;

            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleSeed seed = job.CreateSeed();
            try
            {
                for (int i = 0; i < count; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(
                            "Seed scan", $"{job.Id}: seed {job.Seed + i}", i / (float)count))
                        break;

                    seed.seed = job.Seed + i;
                    entries.Add(Probe(seed, job, settings, sim));
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                UnityEngine.Object.DestroyImmediate(seed);
            }

            entries.Sort(ByUsefulness);
            return entries;
        }

        // Fleet contracts probe off the editor thread; the window polls the run and takes the entries
        // when it finishes. The seed asset lives until then so the run can vary its seed value.
        public static FleetSearchRun ScanFleetAsync(
            PuzzleJobContract job, int count, Action<List<Entry>> onDone)
        {
            SpaceShapingSettings settings = PuzzleTestSetBuilder.LoadSettings("Seed scan");
            if (settings == null)
            {
                onDone(new List<Entry>());
                return null;
            }

            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleSeed seed = job.CreateSeed();
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            List<Entry> entries = new();
            return FleetSearchRun.Start(
                new FleetSearchRequest(seed, settings, brief, job.Seed, count),
                (seedValue, result) =>
                {
                    entries.Add(EntryFor(seedValue, result, job, sim));
                    return true;
                },
                outcome =>
                {
                    UnityEngine.Object.DestroyImmediate(seed);
                    entries.Sort(ByUsefulness);
                    onDone(entries);
                });
        }

        // Seeds that pass the contract first, then the widest solve window.
        private static int ByUsefulness(Entry a, Entry b)
        {
            if (a.MeetsContract != b.MeetsContract)
                return a.MeetsContract ? -1 : 1;

            return b.Window.CompareTo(a.Window);
        }

        private static Entry Probe(
            PuzzleSeed seed, PuzzleJobContract job, SpaceShapingSettings settings, PuzzleSimSettings sim)
        {
            PuzzleSeedScanResult result = PuzzleSeedScanner.Find(seed, settings, 1);
            return EntryFor(seed.seed, result, job, sim);
        }

        private static Entry EntryFor(
            int seedValue, PuzzleSeedScanResult result, PuzzleJobContract job, PuzzleSimSettings sim)
        {
            Entry entry = new()
            {
                Seed = seedValue,
                Baked = result.Found,
                Kind = result.Kind,
                Status = result.Status,
                RunReport = result.RunReport
            };

            if (result.Found)
                Measure(ref entry, result, job, sim);

            if (result.Puzzle != null)
                UnityEngine.Object.DestroyImmediate(result.Puzzle);

            return entry;
        }

        private static void Measure(
            ref Entry entry, PuzzleSeedScanResult result, PuzzleJobContract job, PuzzleSimSettings sim)
        {
            entry.MeetsContract = PuzzleJobValidator.Validate(
                result.Puzzle, result.Kind, result.Solution, job, out List<string> problems, sim);
            entry.Problem = problems.Count > 0 ? problems[0] : string.Empty;
            entry.Window = PuzzleSolveWindow.Measure(result.Puzzle, result.Solution, sim);
        }
    }
}
