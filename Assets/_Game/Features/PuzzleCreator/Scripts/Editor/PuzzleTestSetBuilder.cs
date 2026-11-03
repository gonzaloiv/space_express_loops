using DigitalLove.Casual.Levels;
using DigitalLove.Game.SpaceShaping;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    public static class PuzzleTestSetBuilder
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Resources/SpaceShapingSettings.asset";
        private const string GeneratedFolder = "Assets/_Game/Features/PuzzleCreator/Resources";
        private const string ChapterPath = "Assets/_Game/Resources/Chapters/01.asset";
        private const int MazeSeed = 1;
        private const int MazePlanets = 6;
        private const int MazeAttempts = 8;

        private struct Job
        {
            public readonly string Id;
            public readonly int Seed;
            public readonly int PlanetCount;
            public readonly bool Translatable;
            public readonly int Zones;
            public readonly int MinRoutes;
            public readonly int Attempts;

            public Job(
                string id, int seed, int planetCount, bool translatable, int zones,
                int minRoutes, int attempts)
            {
                Id = id;
                Seed = seed;
                PlanetCount = planetCount;
                Translatable = translatable;
                Zones = zones;
                MinRoutes = minRoutes;
                Attempts = attempts;
            }
        }

        private static readonly Job[] Jobs =
        {
            new Job("01_01", 1, 3, false, 1, 0, 4),
            new Job("01_02", 1, 3, false, 1, 0, 6),
            new Job("01_03", 1, 4, true, 1, 0, 4),
            new Job("01_04", 1, 5, false, 1, 0, 4),
            new Job("01_05", 2, 5, true, 2, 0, 4),
            new Job("01_06", 3, 5, true, 2, 0, 4),
            new Job("01_07", 4, 5, true, 2, 2, 4),
            new Job("01_08", 5, 6, true, 2, 2, 4),
        };

        [MenuItem("DigitalLove/Space Shaping/Build 8-Job Test Set")]
        public static void Build()
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            if (settings == null)
            {
                Debug.LogError($"Test set: missing settings at {SettingsPath}.");
                return;
            }

            PuzzleData[] puzzles = new PuzzleData[Jobs.Length];
            PuzzleData fallback = null;
            for (int i = 0; i < Jobs.Length; i++)
            {
                Job job = Jobs[i];
                PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
                seed.seed = job.Seed;
                seed.planetCount = job.PlanetCount;
                seed.translatable = job.Translatable;
                seed.zones = job.Zones;
                seed.resizable = false;

                PuzzleSeedScanResult result = PuzzleSeedScanner.Find(seed, settings, job.Attempts);
                Object.DestroyImmediate(seed);

                PuzzleData baked = result.Found ? result.Puzzle : null;
                if (baked == null && job.Zones > 1)
                {
                    baked = TryBakeLoose(job, settings, out PuzzleBakeStatus looseStatus);
                    if (baked != null)
                    {
                        Debug.LogWarning(
                            $"Test set: {job.Id} used the loose 2-zone bake ({result.Status} -> " +
                            $"{looseStatus}); a one-zone shortcut may exist.");
                    }
                }

                if (baked == null)
                {
                    if (fallback == null)
                    {
                        Debug.LogError($"Test set: {job.Id} did not bake and there is no fallback.");
                        continue;
                    }

                    baked = Object.Instantiate(fallback);
                    Debug.LogWarning(
                        $"Test set: {job.Id} did not bake ({result.Status}); duplicated {fallback.id}.");
                }

                baked.id = job.Id;
                PuzzleArtifactExporter.SaveVariant(baked, "Job_" + job.Id);
                string path = $"{GeneratedFolder}/Job_{job.Id}_Puzzle.asset";
                PuzzleData saved = AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
                puzzles[i] = saved;
                if (saved != null)
                    fallback = saved;

                Debug.Log(
                    $"Test set: {job.Id} {result.Status} (seed {result.Seed}, {baked.zones} zone(s)) -> " +
                    (saved != null ? "saved" : "MISSING"));
            }

            WireChapter(puzzles);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Test set: {Jobs.Length} jobs built and wired into chapter 01.");
        }

        [MenuItem("DigitalLove/Space Shaping/Build Maze Test Level")]
        public static void BuildMaze()
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            if (settings == null)
            {
                Debug.LogError($"Maze test: missing settings at {SettingsPath}.");
                return;
            }

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
            PuzzleArtifactExporter.SaveVariant(baked, "Maze_01");
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

        private static PuzzleData TryBakeLoose(
            Job job, SpaceShapingSettings settings, out PuzzleBakeStatus status)
        {
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = job.Seed;
            seed.planetCount = job.PlanetCount;
            seed.translatable = job.Translatable;
            seed.zones = job.Zones;
            seed.minRoutes = job.MinRoutes;
            seed.resizable = false;

            PuzzlePlanetListAsset planets = Planets(seed, settings);
            PuzzleBakeResult result = PuzzleFieldBake.BakeLoose(
                planets, PuzzleSimSettings.From(settings), PuzzleBrief.From(seed, settings));
            Object.DestroyImmediate(planets);
            Object.DestroyImmediate(seed);
            status = result.Status;
            return result.Puzzle;
        }

        private static PuzzlePlanetListAsset Planets(PuzzleSeed seed, SpaceShapingSettings settings) =>
            PuzzleArtifactFactory.PlanetsFor(seed, settings, seed.seed);

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
