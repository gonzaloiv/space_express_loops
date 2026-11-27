using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DigitalLove.Game.SpaceShaping;
using UnityEditor;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

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
            Stopwatch wall = Stopwatch.StartNew();
            List<PuzzleJobSeedScan.Entry> entries = PuzzleJobSeedScan.Scan(job, seeds, workers);
            wall.Stop();

            StringBuilder report = new();
            report.AppendLine($"job={job.Id} seeds={seeds} workers={workers}");
            for (int i = 0; i < entries.Count; i++)
                AppendSeed(report, entries[i]);

            string totals = Totals(entries, wall.ElapsedMilliseconds);
            report.AppendLine(totals);
            Debug.Log(totals);
            BakeReport.Write(job.Id, report.ToString());
        }

        private static void AppendSeed(StringBuilder report, PuzzleJobSeedScan.Entry entry)
        {
            string line = string.Format(
                CultureInfo.InvariantCulture,
                "seed={0} baked={1} meets={2} status={3} problem={4} window={5} " +
                "scaleDelta={6} startDistance={7} {8}",
                entry.Seed, entry.Baked, entry.MeetsContract, entry.Status, entry.Problem,
                entry.Window, entry.ScaleDelta, entry.StartDistance, Describe(entry.Timings));
            report.AppendLine(line);
            Debug.Log(line);
        }

        private static string Totals(List<PuzzleJobSeedScan.Entry> entries, long wallMs)
        {
            double[] ms = PhaseTotals(entries, out int simulations, out int measured);
            StringBuilder line = new();
            line.Append(string.Format(
                CultureInfo.InvariantCulture,
                "timings seeds={0} wallMs={1} wallPerSeedMs={2} cpuMs={3:F0} sims={4}",
                measured, wallMs, PerSeed(wallMs, measured), Sum(ms), simulations));
            for (int phase = 0; phase < ms.Length; phase++)
            {
                line.Append(string.Format(
                    CultureInfo.InvariantCulture,
                    " {0}Ms={1:F0}", PuzzleScanTimings.Name(phase), ms[phase]));
            }

            AppendHistogram(line, entries);
            return line.ToString();
        }

        // Phase times add up across workers, so they are CPU milliseconds; wallMs is the run clock.
        private static double[] PhaseTotals(
            List<PuzzleJobSeedScan.Entry> entries, out int simulations, out int measured)
        {
            double[] ms = new double[PuzzleScanTimings.PhaseCount];
            simulations = 0;
            measured = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                PuzzleScanTimings timings = entries[i].Timings;
                if (timings == null)
                    continue;

                measured++;
                simulations += timings.Simulations;
                for (int phase = 0; phase < ms.Length; phase++)
                    ms[phase] += timings.Milliseconds(phase);
            }

            return ms;
        }

        private static double Sum(double[] values)
        {
            double total = 0;
            for (int i = 0; i < values.Length; i++)
                total += values[i];

            return total;
        }

        private static long PerSeed(long wallMs, int measured) =>
            measured > 0 ? wallMs / measured : 0;

        private static void AppendHistogram(
            StringBuilder line, List<PuzzleJobSeedScan.Entry> entries)
        {
            int count = Enum.GetValues(typeof(PuzzleBakeStatus)).Length;
            int[] statuses = new int[count];
            int passing = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].MeetsContract)
                    passing++;

                statuses[(int)entries[i].Status]++;
            }

            line.Append($" passing={passing} statuses=");
            for (int status = 0; status < count; status++)
            {
                if (statuses[status] > 0)
                    line.Append($"{(PuzzleBakeStatus)status}:{statuses[status]},");
            }
        }

        private static string Describe(PuzzleScanTimings timings)
        {
            if (timings == null)
                return "timings=none";

            StringBuilder line = new();
            line.Append($"sims={timings.Simulations}");
            for (int phase = 0; phase < PuzzleScanTimings.PhaseCount; phase++)
            {
                line.Append(string.Format(
                    CultureInfo.InvariantCulture,
                    " {0}Ms={1:F0}",
                    PuzzleScanTimings.Name(phase), timings.Milliseconds(phase)));
            }

            return line.ToString();
        }

        private static void BakeJob(PuzzleJobContract job)
        {
            PuzzleData built = PuzzleTestSetBuilder.BuildOne(job);
            if (built == null)
                throw new InvalidOperationException($"{job.Id} did not bake.");
            string path = PuzzleTestSetBuilder.JobPath(job);
            PuzzleData baked = AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
            if (baked == null || baked.id != job.Id)
                throw new InvalidOperationException($"{job.Id} was not saved at {path}.");

            string answerPath = PuzzleTestSetBuilder.AnswerPath(job);
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
