using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    // One-time (and repeatable) backfill: solve each enabled shipped job once, as the shipped check
    // used to, and store its answer. Jobs that do not solve are listed, not written.
    public static class PuzzleAnswerBackfill
    {
        private const string ChaptersFolder = "Assets/_Game/Resources/Chapters";

        [MenuItem("DigitalLove/Space Shaping/Backfill Job Answers")]
        public static void Backfill() => Run();

        // Batch entry: the bake checkout calls this with -executeMethod and reads the exit code.
        public static void BackfillBatch()
        {
            bool ok = Run();
            EditorApplication.Exit(ok ? 0 : 1);
        }

        private static bool Run()
        {
            SpaceShapingSettings settings = PuzzleTestSetBuilder.LoadSettings("Backfill");
            if (settings == null)
                return false;

            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            List<string> failures = new();
            int written = 0;
            for (int i = 0; i < PuzzleJobContracts.Enabled.Count; i++)
            {
                if (Solve(PuzzleJobContracts.Enabled[i], settings, sim, failures))
                    written++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return Report(written, failures);
        }

        private static bool Solve(
            PuzzleJobContract contract, SpaceShapingSettings settings, PuzzleSimSettings sim,
            List<string> failures)
        {
            PuzzleData puzzle = LoadShipped(contract.Id);
            if (puzzle == null)
            {
                failures.Add($"{contract.Id}: missing shipped asset");
                return false;
            }

            PuzzleJobSolver.AnswerAttempt attempt =
                PuzzleJobSolver.TrySolveAnswer(puzzle, contract, settings, sim);
            if (!attempt.Found)
            {
                failures.Add($"{contract.Id}: no answer meets the contract: {Problems(attempt)}");
                return false;
            }

            PuzzleAnswerAsset answer = PuzzleAnswerFactory.From(
                contract, contract.Seed, puzzle, attempt.Solution, sim);
            PuzzleArtifactExporter.SaveAnswer(answer);
            return true;
        }

        private static string Problems(PuzzleJobSolver.AnswerAttempt attempt) =>
            attempt.Problems.Count == 0
                ? "no solution found"
                : string.Join("; ", attempt.Problems);

        private static PuzzleData LoadShipped(string id)
        {
            string chapter = PuzzleJobContracts.ChapterOf(id);
            string path = $"{ChaptersFolder}/{chapter}/Job_{id}_Puzzle.asset";
            return AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
        }

        private static bool Report(int written, List<string> failures)
        {
            StringBuilder report = new();
            report.AppendLine($"answers written={written} failed={failures.Count}");
            for (int i = 0; i < failures.Count; i++)
                report.AppendLine(failures[i]);
            BakeReport.Write("answers", report.ToString());

            if (failures.Count == 0)
            {
                Debug.Log($"Answers: {written} stored.");
                return true;
            }

            Debug.LogError($"Answers: {failures.Count} job(s) did not solve:\n{report}");
            return false;
        }
    }
}
