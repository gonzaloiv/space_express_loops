using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    public class PuzzleJobBakeWindow : EditorWindow
    {
        private const int MaxShown = 10;

        private int scanCount = 30;
        private string scannedId;
        private int contractSeed;
        private List<PuzzleJobSeedScan.Entry> scan;
        private FleetSearchRun fleet;
        private Vector2 scroll;
        private PuzzleJobChecks checks;

        private PuzzleJobChecks Checks
        {
            get
            {
                if (checks != null)
                    return checks;

                checks = new PuzzleJobChecks();
                checks.Changed += Repaint;
                return checks;
            }
        }

        [MenuItem("DigitalLove/Space Shaping/Job Baker")]
        private static void Open() => GetWindow<PuzzleJobBakeWindow>("Job Baker");

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            scanCount = EditorGUILayout.IntSlider("Seeds per scan", scanCount, 1, 1000);
            IReadOnlyList<PuzzleJobContract> jobs = PuzzleJobContracts.Enabled;
            using (new EditorGUI.DisabledScope(FleetSearchRun.IsRunning))
            {
                DrawChapters(jobs);

                EditorGUILayout.Space();
                if (GUILayout.Button("Bake all"))
                    PuzzleTestSetBuilder.Build();
            }

            DrawChecks();
            DrawScan();
            EditorGUILayout.EndScrollView();
        }

        private void DrawChapters(IReadOnlyList<PuzzleJobContract> jobs)
        {
            string chapter = null;
            for (int i = 0; i < jobs.Count; i++)
            {
                string next = PuzzleJobContracts.ChapterOf(jobs[i].Id);
                if (next != chapter)
                {
                    chapter = next;
                    EditorGUILayout.LabelField($"Chapter {chapter} jobs", EditorStyles.boldLabel);
                }

                DrawJob(jobs[i]);
            }
        }

        private void DrawJob(PuzzleJobContract job)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    $"{job.Id} · {job.Mechanism} · {job.PlanetCount} planets · seed {job.Seed}");
                if (GUILayout.Button("Scan", GUILayout.Width(60f)))
                    RunScan(job);
                if (GUILayout.Button("Bake", GUILayout.Width(60f)))
                    PuzzleTestSetBuilder.BuildOne(job);
            }
        }

        // After a bake: the sun-answer tests and the job set against its contracts (the slow one).
        private void DrawChecks()
        {
            using (new EditorGUI.DisabledScope(Checks.Running))
            {
                if (GUILayout.Button("Run job checks (sun answer + job set)"))
                    Checks.Run();
            }

            if (Checks.Summary.Length > 0)
                EditorGUILayout.HelpBox(Checks.Summary, MessageType.None);
        }

        private void RunScan(PuzzleJobContract job)
        {
            scannedId = job.Id;
            contractSeed = job.Seed;
            scan = null;
            if (FleetSearchRun.IsRunning)
            {
                Debug.LogWarning("Job baker: a fleet search is already running.");
                return;
            }

            if (!IsFleet(job))
            {
                scan = PuzzleJobSeedScan.Scan(job, scanCount);
                return;
            }

            fleet = PuzzleJobSeedScan.ScanFleetAsync(job, scanCount, OnFleetScan);
        }

        // A fleet contract is one whose seed bakes more than one ship; until the contract carries ships
        // this is false, so the synchronous path stays for chapter 01.
        private static bool IsFleet(PuzzleJobContract job)
        {
            SpaceShapingSettings settings = PuzzleTestSetBuilder.LoadSettings("Seed scan");
            if (settings == null)
                return false;

            PuzzleSeed seed = job.CreateSeed();
            bool multi = PuzzleBrief.From(seed, settings).IsMulti;
            Object.DestroyImmediate(seed);
            return multi;
        }

        private void OnFleetScan(List<PuzzleJobSeedScan.Entry> entries)
        {
            fleet = null;
            scan = entries;
            Repaint();
        }

        private void OnDisable()
        {
            fleet?.Cancel();
        }

        // The window is the run of arriving scales around the solution, as a share of the scale range:
        // the higher, the more forgiving. Only ✓ seeds pass the contract test; pin one as the contract's
        // Seed, then Bake. ✗ seeds bake but break a contract rule, named after the dash.
        private void DrawScan()
        {
            if (fleet != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    $"Scanning {scannedId}: {fleet.SeedsDone}/{fleet.SeedsTotal} seeds · " +
                    $"{fleet.ZonesDone} zones", EditorStyles.boldLabel);
                if (GUILayout.Button("Cancel scan"))
                    fleet.Cancel();
                return;
            }

            if (scan == null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                $"Scan {scannedId} — contract passes first, then widest solve window", EditorStyles.boldLabel);
            int shown = 0;
            for (int i = 0; i < scan.Count && shown < MaxShown; i++)
            {
                if (!scan[i].Baked)
                    continue;

                EditorGUILayout.LabelField(ScanLine(scan[i]));
                shown++;
            }

            EditorGUILayout.LabelField($"{shown} shown · {PassSummary()} · {FailureSummary()}");
            EditorGUILayout.LabelField($"NoRun pairs: {NoRunReport()}");
        }

        private string ScanLine(PuzzleJobSeedScan.Entry entry)
        {
            string mark = entry.Seed == contractSeed ? "  ← contract" : string.Empty;
            string line = $"seed {entry.Seed} · window {entry.Window:P0} · {entry.Kind}{mark}";
            return entry.MeetsContract ? $"✓ {line}" : $"✗ {line} — {entry.Problem}";
        }

        private string PassSummary()
        {
            int passing = 0;
            int broke = 0;
            for (int i = 0; i < scan.Count; i++)
            {
                if (scan[i].MeetsContract)
                    passing++;
                else if (scan[i].Baked)
                    broke++;
            }

            return $"{passing} meet the contract · {broke} bake but break it";
        }

        // Why the run picker rejected the pairs on NoRun seeds, summed over the scan.
        private string NoRunReport()
        {
            PuzzleRunReport total = default;
            for (int i = 0; i < scan.Count; i++)
            {
                if (scan[i].Status == PuzzleBakeStatus.NoRun)
                    total.Add(scan[i].RunReport);
            }

            return total.ToString();
        }

        private string FailureSummary()
        {
            int[] counts = new int[System.Enum.GetValues(typeof(PuzzleBakeStatus)).Length];
            int failed = 0;
            for (int i = 0; i < scan.Count; i++)
            {
                if (scan[i].Baked)
                    continue;

                counts[(int)scan[i].Status]++;
                failed++;
            }

            StringBuilder text = new($"{failed} seeds did not bake");
            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] > 0)
                    text.Append($" · {(PuzzleBakeStatus)i} {counts[i]}");
            }

            return text.ToString();
        }
    }
}
