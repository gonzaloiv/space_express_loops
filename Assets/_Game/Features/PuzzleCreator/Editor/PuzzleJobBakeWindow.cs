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
        private Vector2 scroll;

        [MenuItem("DigitalLove/Space Shaping/Job Baker")]
        private static void Open() => GetWindow<PuzzleJobBakeWindow>("Job Baker");

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Chapter 01 jobs", EditorStyles.boldLabel);
            scanCount = EditorGUILayout.IntSlider("Seeds per scan", scanCount, 1, 200);
            IReadOnlyList<PuzzleJobContract> jobs = PuzzleJobContracts.Enabled;
            for (int i = 0; i < jobs.Count; i++)
                DrawJob(jobs[i]);

            EditorGUILayout.Space();
            if (GUILayout.Button("Bake all"))
                PuzzleTestSetBuilder.Build();

            DrawScan();
            EditorGUILayout.EndScrollView();
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

        private void RunScan(PuzzleJobContract job)
        {
            scannedId = job.Id;
            contractSeed = job.Seed;
            scan = PuzzleJobSeedScan.Scan(job, scanCount);
        }

        // The window is the run of arriving scales around the solution, as a share of the scale range:
        // the higher, the more forgiving. Pin the chosen seed as the contract's Seed, then Bake.
        private void DrawScan()
        {
            if (scan == null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                $"Scan {scannedId} — widest solve window first", EditorStyles.boldLabel);
            int shown = 0;
            for (int i = 0; i < scan.Count && shown < MaxShown; i++)
            {
                PuzzleJobSeedScan.Entry entry = scan[i];
                if (!entry.Baked)
                    continue;

                string mark = entry.Seed == contractSeed ? "  ← contract" : string.Empty;
                EditorGUILayout.LabelField(
                    $"seed {entry.Seed} · window {entry.Window:P0} · {entry.Kind}{mark}");
                shown++;
            }

            EditorGUILayout.LabelField($"{shown} shown · {FailureSummary()}");
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
