using System;
using System.Collections.Generic;
using System.Text;
using DigitalLove.Game.SpaceShaping;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    public static class PuzzleBatchBake
    {
        private const string ArgumentJob = "-job";
        private const string ArgumentSeeds = "-seeds";
        private const string ArgumentWorkers = "-workers";

        public static void Scan() => Run(false);

        public static void Bake() => Run(true);

        private static void Run(bool bake)
        {
            try
            {
                string id = Required(ArgumentJob);
                int seeds = bake ? 1 : ParseSeeds();
                int workers = bake ? 1 : ParseWorkers();
                PuzzleJobContract job = PuzzleJobContracts.ById(id);
                if (bake)
                    BakeJob(job);
                else
                    ScanJob(job, seeds, workers);

                AssetDatabase.SaveAssets();
                EditorApplication.Exit(0);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                BakeReport.Write("error", error.ToString());
                EditorApplication.Exit(1);
            }
        }

        private static void ScanJob(PuzzleJobContract job, int seeds, int workers)
        {
            List<PuzzleJobSeedScan.Entry> entries = PuzzleJobSeedScan.Scan(job, seeds, workers);
            StringBuilder report = new();
            report.AppendLine($"job={job.Id} seeds={seeds} workers={workers}");
            for (int i = 0; i < entries.Count; i++)
            {
                PuzzleJobSeedScan.Entry entry = entries[i];
                report.AppendLine(
                    $"seed={entry.Seed} baked={entry.Baked} meets={entry.MeetsContract} " +
                    $"status={entry.Status} problem={entry.Problem} window={entry.Window} " +
                    $"scaleDelta={entry.ScaleDelta} startDistance={entry.StartDistance}");
            }

            BakeReport.Write(job.Id, report.ToString());
        }

        private static void BakeJob(PuzzleJobContract job)
        {
            PuzzleTestSetBuilder.BuildOne(job);
            string chapter = PuzzleJobContracts.ChapterOf(job.Id);
            string path = $"Assets/_Game/Resources/Chapters/{chapter}/Job_{job.Id}_Puzzle.asset";
            PuzzleData baked = AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
            if (baked == null || baked.id != job.Id)
                throw new InvalidOperationException($"{job.Id} was not saved at {path}.");

            string answerPath = PuzzleAnswerAsset.PathFor(job.Id);
            PuzzleAnswerAsset answer = AssetDatabase.LoadAssetAtPath<PuzzleAnswerAsset>(answerPath);
            if (answer == null || answer.id != job.Id)
                throw new InvalidOperationException($"{job.Id} has no answer at {answerPath}.");

            BakeReport.Write(job.Id, $"job={job.Id} baked seed={job.Seed} path={path}");
        }

        private static int ParseSeeds()
        {
            string value = Required(ArgumentSeeds);
            if (!int.TryParse(value, out int seeds) || seeds < 1)
                throw new ArgumentException($"{ArgumentSeeds} must be a positive integer.");

            return seeds;
        }

        private static int ParseWorkers()
        {
            string value = Argument(ArgumentWorkers);
            if (string.IsNullOrEmpty(value))
                return PuzzleSeedBatch.DefaultWorkers;

            if (!int.TryParse(value, out int workers) || workers < 1)
                throw new ArgumentException($"{ArgumentWorkers} must be a positive integer.");

            return workers;
        }

        private static string Required(string name)
        {
            string value = Argument(name);
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"Missing {name} argument.");

            return value;
        }

        private static string Argument(string name)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                if (arguments[i] == name)
                    return arguments[i + 1];
            }

            return null;
        }
    }
}
