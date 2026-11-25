using System;
using System.Collections.Generic;
using System.IO;
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

        public static void Scan() => Run(false);

        public static void Bake() => Run(true);

        private static void Run(bool bake)
        {
            try
            {
                string id = Required(ArgumentJob);
                int seeds = bake ? 1 : ParseSeeds();
                PuzzleJobContract job = PuzzleJobContracts.ById(id);
                if (bake)
                    BakeJob(job);
                else
                    ScanJob(job, seeds);

                AssetDatabase.SaveAssets();
                EditorApplication.Exit(0);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                WriteReport("error", error.ToString());
                EditorApplication.Exit(1);
            }
        }

        private static void ScanJob(PuzzleJobContract job, int seeds)
        {
            List<PuzzleJobSeedScan.Entry> entries = PuzzleJobSeedScan.Scan(job, seeds);
            StringBuilder report = new();
            report.AppendLine($"job={job.Id} seeds={seeds}");
            for (int i = 0; i < entries.Count; i++)
            {
                PuzzleJobSeedScan.Entry entry = entries[i];
                report.AppendLine(
                    $"seed={entry.Seed} baked={entry.Baked} meets={entry.MeetsContract} " +
                    $"status={entry.Status} problem={entry.Problem} window={entry.Window} " +
                    $"scaleDelta={entry.ScaleDelta} startDistance={entry.StartDistance}");
            }

            WriteReport(job.Id, report.ToString());
        }

        private static void BakeJob(PuzzleJobContract job)
        {
            PuzzleTestSetBuilder.BuildOne(job);
            string chapter = PuzzleJobContracts.ChapterOf(job.Id);
            string path = $"Assets/_Game/Resources/Chapters/{chapter}/Job_{job.Id}_Puzzle.asset";
            PuzzleData baked = AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
            if (baked == null || baked.id != job.Id)
                throw new InvalidOperationException($"{job.Id} was not saved at {path}.");

            WriteReport(job.Id, $"job={job.Id} baked seed={job.Seed} path={path}");
        }

        private static int ParseSeeds()
        {
            string value = Required(ArgumentSeeds);
            if (!int.TryParse(value, out int seeds) || seeds < 1)
                throw new ArgumentException($"{ArgumentSeeds} must be a positive integer.");

            return seeds;
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

        private static void WriteReport(string name, string text)
        {
            string folder = Path.Combine(ProjectRoot(), "Logs");
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, $"bake-{name}.txt"), text);
        }

        private static string ProjectRoot() =>
            Path.GetDirectoryName(Application.dataPath) ?? Directory.GetCurrentDirectory();
    }
}
