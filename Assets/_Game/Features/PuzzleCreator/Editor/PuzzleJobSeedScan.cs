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
            // The numbers a high-scramble contract pins: the answer's |scale - 1| and, on a
            // movable job, how far the start frame sits from it.
            public float ScaleDelta;
            public float StartDistance;
            public PuzzleSolutionKind Kind;
            public PuzzleBakeStatus Status;
            public PuzzleRunReport RunReport;
        }

        public static List<Entry> Scan(PuzzleJobContract job, int count, int workers = 1)
        {
            List<Entry> entries = new();
            SpaceShapingSettings settings = PuzzleTestSetBuilder.LoadSettings("Seed scan");
            if (settings == null)
                return entries;

            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleSeed seed = job.CreateSeed();
            try
            {
                List<PuzzleSeedScanResult> results = ScanResults(job, settings, seed, count, workers);
                for (int i = 0; i < results.Count; i++)
                    entries.Add(EntryFor(results[i].Seed, results[i], job, sim));
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                UnityEngine.Object.DestroyImmediate(seed);
            }

            entries.Sort(ByUsefulness);
            return entries;
        }

        private static List<PuzzleSeedScanResult> ScanResults(
            PuzzleJobContract job, SpaceShapingSettings settings, PuzzleSeed seed, int count, int workers)
        {
            if (job.Generator == PuzzleGenerator.Construct)
                return Sequential(seed, settings, job, count);

            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            Func<int, IPuzzleSeedSearch> prepare = seedValue =>
                PuzzleMultiShipSearch.PrepareForSeed(seed, settings, brief, seedValue);
            if (brief.IsMulti && workers > 1)
                return Fleet(prepare, job, count, workers);

            return Sequential(seed, settings, job, count);
        }

        // A single-ship scan stays on the calling thread: its bake searches through assets, not a
        // snapshot, so there is nothing to hand to a worker.
        private static List<PuzzleSeedScanResult> Sequential(
            PuzzleSeed seed, SpaceShapingSettings settings, PuzzleJobContract job, int count)
        {
            List<PuzzleSeedScanResult> results = new();
            for (int i = 0; i < count; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar(
                        "Seed scan", $"{job.Id}: seed {job.Seed + i}", i / (float)count))
                    break;

                seed.seed = job.Seed + i;
                results.Add(Probe(job, seed, settings));
            }

            return results;
        }

        // Fleets run their probes on one task per worker; the batch confirms them in seed order.
        private static List<PuzzleSeedScanResult> Fleet(
            Func<int, IPuzzleSeedSearch> prepare, PuzzleJobContract job, int count, int workers)
        {
            PuzzleSeedBatch batch = new(prepare, job.Seed, count, workers);
            return batch.Run((done, total) =>
            {
                bool stopped = EditorUtility.DisplayCancelableProgressBar(
                    "Seed scan", $"{job.Id}: {done}/{total} seeds", done / (float)total);
                if (stopped)
                    batch.Cancel();
            });
        }

        // Fleet contracts probe off the editor thread; the window polls the run and takes the entries
        // when it finishes. The seed asset lives until then so the run can vary its seed value.
        // A construct bake is main-thread (a PuzzleData and PlanetNames.Instance), so a Construct fleet
        // runs the synchronous scan and reports it at once instead of a worker search.
        public static FleetSearchRun ScanFleetAsync(
            PuzzleJobContract job, int count, Action<List<Entry>> onDone)
        {
            if (job.Generator == PuzzleGenerator.Construct)
            {
                onDone(Scan(job, count));
                return null;
            }

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
                    entries.Sort(ByUsefulness);
                    onDone(entries);
                },
                () => UnityEngine.Object.DestroyImmediate(seed));
        }

        // Seeds that pass the contract first, then the widest solve window.
        private static int ByUsefulness(Entry a, Entry b)
        {
            if (a.MeetsContract != b.MeetsContract)
                return a.MeetsContract ? -1 : 1;

            return b.Window.CompareTo(a.Window);
        }

        private static PuzzleSeedScanResult Probe(
            PuzzleJobContract job, PuzzleSeed seed, SpaceShapingSettings settings) =>
            PuzzleSeedScanner.Find(
                job, seed, settings,
                job.Generator == PuzzleGenerator.Construct ? Mathf.Max(1, job.Attempts) : 1);

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
            if (result.Problems != null && result.Problems.Count > 0)
                entry.Problem = result.Problems[0];

            if (result.Found)
                Measure(ref entry, result, job, sim);

            if (result.Puzzle != null)
                UnityEngine.Object.DestroyImmediate(result.Puzzle);

            return entry;
        }

        private static void Measure(
            ref Entry entry, PuzzleSeedScanResult result, PuzzleJobContract job, PuzzleSimSettings sim)
        {
            if (result.ContractChecked)
            {
                entry.MeetsContract = result.Problems == null || result.Problems.Count == 0;
                entry.Problem = result.Problems != null && result.Problems.Count > 0
                    ? result.Problems[0]
                    : string.Empty;
            }
            else
            {
                entry.MeetsContract = PuzzleJobValidator.Validate(
                    result.Puzzle, result.Kind, result.Solution, job, out List<string> problems, sim);
                entry.Problem = problems.Count > 0 ? problems[0] : string.Empty;
            }

            entry.Window = result.Window > 0f
                ? result.Window
                : PuzzleSolveWindow.Measure(result.Puzzle, result.Solution, sim);
            MeasureScramble(ref entry, result);
        }

        // From the baked answer: the shipped check replays the same stored zone. The contract's bounds
        // are pinned just under these.
        private static void MeasureScramble(ref Entry entry, PuzzleSeedScanResult result)
        {
            if (result.Solution == null || result.Solution.Length == 0)
                return;

            Vector3 centre = result.Solution[0].centre;
            entry.ScaleDelta = Mathf.Abs(result.Solution[0].scale - 1f);
            if (result.Puzzle.frames == null || result.Puzzle.frames.Length == 0)
                return;

            entry.StartDistance = Vector3.Distance(result.Puzzle.frames[0].centre, centre);
        }
    }
}
