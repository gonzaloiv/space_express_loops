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
        private const string GeneratedFolder = "Assets/_Game/Resources/Chapters/01";
        private const string ChapterPath = "Assets/_Game/Resources/Chapters/01.asset";
        private const int MazeSeed = 1;
        private const int MazePlanets = 6;
        private const int MazeAttempts = 8;

        [MenuItem("DigitalLove/Space Shaping/Build 8-Job Test Set")]
        public static void Build()
        {
            SpaceShapingSettings settings = LoadSettings("Test set");
            if (settings == null)
                return;

            IReadOnlyList<PuzzleJobContract> jobs = PuzzleJobContracts.All;
            PuzzleData[] puzzles = new PuzzleData[jobs.Count];
            for (int i = 0; i < jobs.Count; i++)
                puzzles[i] = BuildJob(jobs[i], settings);

            WireChapter(puzzles);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Test set: {jobs.Count} jobs built and wired into chapter 01.");
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
            PuzzleArtifactExporter.SaveVariant(baked, "Maze_01", GeneratedFolder);
            string path = $"{GeneratedFolder}/Maze_01_Puzzle.asset";
            PuzzleData saved = AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
            if (saved == null)
            {
                Debug.LogError($"Maze test: baked but not saved at {path}.");
                return;
            }

            WireChapter(new[] { saved });
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Maze test: baked Maze_01 (seed {result.Seed}, {baked.zones} zone(s)) and wired into chapter 01.");
        }

        private static PuzzleData BuildJob(PuzzleJobContract job, SpaceShapingSettings settings)
        {
            PuzzleSeed seed = CreateSeed(job);
            PuzzleSeedScanResult result = FindJob(job, settings, seed);
            Object.DestroyImmediate(seed);

            if (!result.Found)
            {
                Debug.LogError($"Test set: {job.Id} did not bake ({result.Status}).");
                return null;
            }

            PuzzleData baked = result.Puzzle;
            baked.id = job.Id;
            PuzzleArtifactExporter.SaveVariant(baked, "Job_" + job.Id, GeneratedFolder);
            string path = $"{GeneratedFolder}/Job_{job.Id}_Puzzle.asset";
            PuzzleData saved = AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
            Debug.Log(
                $"Test set: {job.Id} {result.Status} (seed {result.Seed}, {baked.zones} zone(s), " +
                $"{result.Kind}) -> " + (saved != null ? "saved" : "MISSING"));
            return saved;
        }

        private static PuzzleSeed CreateSeed(PuzzleJobContract job)
        {
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = job.Seed;
            seed.planetCount = job.PlanetCount;
            seed.translatable = job.Translatable;
            seed.startRule = job.StartRule;
            seed.zones = job.Zones;
            seed.minRoutes = job.MinRoutes;
            seed.mechanism = job.Mechanism;
            seed.hasSun = job.HasSun;
            seed.sunRule = job.SunRule;
            seed.noFail = job.NoFail;
            seed.noGrade = job.NoGrade;
            seed.fieldShape = job.FieldShape;
            seed.resizable = false;
            return seed;
        }

        private static SpaceShapingSettings LoadSettings(string label)
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            if (settings == null)
                Debug.LogError($"{label}: missing settings at {SettingsPath}.");
            return settings;
        }

        private static PuzzleSeedScanResult FindJob(
            PuzzleJobContract job, SpaceShapingSettings settings, PuzzleSeed seed)
        {
            seed.mechanism = job.Mechanism;
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

        private static void WireChapter(PuzzleData[] puzzles)
        {
            ChapterData chapter = AssetDatabase.LoadAssetAtPath<ChapterData>(ChapterPath);
            if (chapter == null)
            {
                Debug.LogError($"Test set: missing chapter at {ChapterPath}.");
                return;
            }

            int count = 0;
            LevelData[] levels = new LevelData[puzzles.Length];
            for (int i = 0; i < puzzles.Length; i++)
            {
                if (puzzles[i] == null)
                    continue;

                levels[count++] = puzzles[i];
            }

            if (count != levels.Length)
                System.Array.Resize(ref levels, count);

            chapter.levels = levels;
            EditorUtility.SetDirty(chapter);
        }
    }
}
