using System.Collections.Generic;
using DigitalLove.Casual.Levels;
using DigitalLove.Game.SpaceShaping;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    public static class PuzzleTestSetBuilder
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";
        private const string ChaptersFolder = "Assets/_Game/Resources/Chapters";
        private const int MazeSeed = 1;
        private const int MazePlanets = 6;
        private const int MazeAttempts = 8;

        [MenuItem("DigitalLove/Space Shaping/Build Job Test Set")]
        public static void Build()
        {
            SpaceShapingSettings settings = LoadSettings("Test set");
            if (settings == null)
                return;

            IReadOnlyList<PuzzleJobContract> jobs = PuzzleJobContracts.Enabled;
            List<string> chapters = Chapters(jobs);
            for (int i = 0; i < chapters.Count; i++)
                BuildChapter(chapters[i], jobs, settings);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Test set: {jobs.Count} jobs built across {chapters.Count} chapter(s).");
        }

        public static void BuildOne(PuzzleJobContract job)
        {
            SpaceShapingSettings settings = LoadSettings("Test set");
            if (settings == null)
                return;

            PuzzleData saved = BuildJob(job, settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(saved != null
                ? $"Test set: {job.Id} rebuilt in chapter {PuzzleJobContracts.ChapterOf(job.Id)}."
                : $"Test set: {job.Id} did not build.");
        }

        [MenuItem("DigitalLove/Space Shaping/Build Maze Test Level")]
        public static void BuildMaze()
        {
            SpaceShapingSettings settings = LoadSettings("Maze test");
            if (settings == null)
                return;

            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = MazeSeed;
            seed.planetCount = MazePlanets;
            seed.translatable = false;
            seed.zones = 1;
            seed.resizable = false;

            PuzzleSeedScanResult result = PuzzleSeedScanner.Find(seed, settings, MazeAttempts);
            Object.DestroyImmediate(seed);

            if (!result.Found)
            {
                Debug.LogError($"Maze test: no maze level baked ({result.Status}).");
                return;
            }

            PuzzleData baked = result.Puzzle;
            baked.id = "MZ_01";
            string folder = GeneratedFolder("01");
            PuzzleArtifactExporter.SaveVariant(baked, "Maze_01", folder);
            string path = $"{folder}/Maze_01_Puzzle.asset";
            PuzzleData saved = AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
            if (saved == null)
            {
                Debug.LogError($"Maze test: baked but not saved at {path}.");
                return;
            }

            WireChapterLevels("01", new[] { saved });
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Maze test: baked Maze_01 (seed {result.Seed}, {baked.zones} zone(s)) and wired into chapter 01.");
        }

        public static SpaceShapingSettings LoadSettings(string label)
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            if (settings == null)
                Debug.LogError($"{label}: missing settings at {SettingsPath}.");
            return settings;
        }

        private static void BuildChapter(
            string chapter, IReadOnlyList<PuzzleJobContract> jobs, SpaceShapingSettings settings)
        {
            int built = 0;
            for (int i = 0; i < jobs.Count; i++)
            {
                if (PuzzleJobContracts.ChapterOf(jobs[i].Id) != chapter)
                    continue;

                if (BuildJob(jobs[i], settings) != null)
                    built++;
            }

            Debug.Log($"Test set: chapter {chapter} — {built} job(s) built.");
        }

        private static PuzzleData BuildJob(PuzzleJobContract job, SpaceShapingSettings settings)
        {
            PuzzleSeed seed = job.CreateSeed();
            PuzzleSeedScanResult result = FindJob(job, settings, seed);
            Object.DestroyImmediate(seed);

            if (!result.Found)
            {
                Debug.LogError($"Test set: {job.Id} did not bake ({result.Status}).");
                return null;
            }

            PuzzleData baked = result.Puzzle;
            baked.id = job.Id;
            string chapter = PuzzleJobContracts.ChapterOf(job.Id);
            PuzzleArtifactExporter.SaveVariant(baked, "Job_" + job.Id, GeneratedFolder(chapter));
            PuzzleData saved = AssetDatabase.LoadAssetAtPath<PuzzleData>(JobPath(job.Id));
            SaveAnswer(job, result, settings);
            Debug.Log(
                $"Test set: {job.Id} {result.Status} (seed {result.Seed}, {baked.zones} zone(s), " +
                $"{result.Kind}) -> " + (saved != null ? $"saved in chapter {chapter}" : "MISSING"));
            return saved;
        }

        private static void SaveAnswer(
            PuzzleJobContract job, PuzzleSeedScanResult result, SpaceShapingSettings settings)
        {
            PuzzleSolution solution = new() { Zones = result.Solution, Kind = result.Kind };
            PuzzleAnswerAsset answer = PuzzleAnswerFactory.From(
                job, result.Seed, result.Puzzle, solution, PuzzleSimSettings.From(settings));
            PuzzleArtifactExporter.SaveAnswer(answer);
        }

        private static PuzzleSeedScanResult FindJob(
            PuzzleJobContract job, SpaceShapingSettings settings, PuzzleSeed seed)
        {
            seed.mechanism = job.Mechanism;
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            if (brief.IsMulti && job.Attempts > 1)
                return FindFleetJob(job, settings, seed, brief);

            PuzzleSeedScanResult result = default;
            for (int attempt = 0; attempt < job.Attempts; attempt++)
            {
                seed.seed = job.Seed + attempt;
                PuzzleSeedScanResult candidate = PuzzleSeedScanner.Find(seed, settings, 1);
                if (candidate.Found)
                    return candidate;

                result = candidate;
            }

            result.Found = false;
            return result;
        }

        // A fleet bake's attempt range runs in parallel and stops on the first pass, so a bake does
        // not wait for seeds after the one it saves.
        private static PuzzleSeedScanResult FindFleetJob(
            PuzzleJobContract job, SpaceShapingSettings settings, PuzzleSeed seed, PuzzleBrief brief)
        {
            PuzzleSeedBatch batch = new(
                seedValue => PuzzleMultiShipSearch.PrepareForSeed(seed, settings, brief, seedValue),
                job.Seed, job.Attempts, PuzzleSeedBatch.DefaultWorkers, stopOnFirst: true);
            List<PuzzleSeedScanResult> results = batch.Run();
            return results.Count > 0 ? results[results.Count - 1] : default;
        }

        private static void WireChapterLevels(string chapter, PuzzleData[] puzzles)
        {
            ChapterData data = LoadChapter(chapter);
            if (data == null)
                return;

            data.levels = puzzles;
            EditorUtility.SetDirty(data);
        }

        private static ChapterData LoadChapter(string chapter)
        {
            string path = $"{ChaptersFolder}/{chapter}.asset";
            ChapterData data = AssetDatabase.LoadAssetAtPath<ChapterData>(path);
            if (data == null)
                Debug.LogError($"Test set: missing chapter at {path}.");
            return data;
        }

        private static List<string> Chapters(IReadOnlyList<PuzzleJobContract> jobs)
        {
            List<string> chapters = new();
            for (int i = 0; i < jobs.Count; i++)
            {
                string chapter = PuzzleJobContracts.ChapterOf(jobs[i].Id);
                if (!chapters.Contains(chapter))
                    chapters.Add(chapter);
            }

            return chapters;
        }

        private static string GeneratedFolder(string chapter) => $"{ChaptersFolder}/{chapter}";

        private static string JobPath(string id) =>
            $"{GeneratedFolder(PuzzleJobContracts.ChapterOf(id))}/Job_{id}_Puzzle.asset";
    }
}
