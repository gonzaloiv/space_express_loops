using DigitalLove.Game.SpaceShaping;
using Oculus.Interaction;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    public static class PuzzleTestSetBuilder
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Resources/SpaceShapingSettings.asset";
        private const string GeneratedFolder = "Assets/_Game/Features/SpaceShaping/Generated";
        private const string ScenePath = "Assets/_Game/Features/SpaceShaping/SpaceShapingPOC.unity";
        private const int MazeSeed = 1;
        private const int MazePlanets = 6;
        private const int MazeAttempts = 8;

        private struct Job
        {
            public readonly string Id;
            public readonly int Seed;
            public readonly int PlanetCount;
            public readonly bool Translatable;
            public readonly int Moves;
            public readonly int Attempts;

            public Job(
                string id, int seed, int planetCount, bool translatable, int moves, int attempts)
            {
                Id = id;
                Seed = seed;
                PlanetCount = planetCount;
                Translatable = translatable;
                Moves = moves;
                Attempts = attempts;
            }
        }

        private static readonly Job[] Jobs =
        {
            new Job("01_01", 1, 3, false, 1, 4),
            new Job("01_02", 1, 3, false, 1, 6),
            new Job("01_03", 1, 4, true, 1, 4),
            new Job("01_04", 1, 5, false, 1, 4),
            new Job("01_05", 2, 5, true, 2, 4),
            new Job("01_06", 3, 5, true, 2, 4),
            new Job("01_07", 4, 5, true, 2, 4),
            new Job("01_08", 5, 6, true, 2, 4),
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
                seed.moves = job.Moves;
                seed.resizable = false;

                PuzzleSeedScanResult result = PuzzleSeedScanner.Find(seed, settings, job.Attempts);
                Object.DestroyImmediate(seed);

                PuzzleData baked = result.Found ? result.Puzzle : null;
                if (baked == null && job.Moves > 1)
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
                    $"Test set: {job.Id} {result.Status} (seed {result.Seed}, {baked.moves} step(s)) -> " +
                    (saved != null ? "saved" : "MISSING"));
            }

            WireScene(puzzles);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Test set: 8 jobs built and wired into SpaceShapingPOC.");
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
            seed.moves = 1;
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

            WireScene(new[] { saved });
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Maze test: baked Maze_01 (seed {result.Seed}, {baked.moves} step(s)) and wired into SpaceShapingPOC.");
        }

        private static PuzzleData TryBakeLoose(
            Job job, SpaceShapingSettings settings, out PuzzleBakeStatus status)
        {
            status = PuzzleBakeStatus.NoSolution;
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = job.Seed;
            seed.planetCount = job.PlanetCount;
            seed.translatable = job.Translatable;
            seed.moves = job.Moves;
            seed.resizable = false;

            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            PuzzlePlanetListAsset planets = Planets(seed, settings);
            bool applied = PuzzleFieldBake.Apply(brief, field, planets, sim);
            Object.DestroyImmediate(planets);
            if (!applied)
            {
                status = PuzzleBakeStatus.NoRun;
                Object.DestroyImmediate(field);
                Object.DestroyImmediate(seed);
                return null;
            }

            if (!PuzzleSolutionSearch.TrySelect(field, sim, brief, out PuzzleSolution found))
            {
                Object.DestroyImmediate(field);
                Object.DestroyImmediate(seed);
                return null;
            }

            field.entropyBudget = found.EntropySpent * PuzzleGrade.BudgetFactor;
            PuzzleData variant = Object.Instantiate(field);
            variant.zoneCentre = brief.Translatable ? field.zoneCentre : found.Zones[0].centre;
            variant.cubeSize = found.Zones[0].halfExtents.x * 2f;
            variant.moves = found.Zones.Length;
            PuzzleVerification check = PuzzleVerifier.Verify(variant, found.Zones, sim);
            Object.DestroyImmediate(field);
            Object.DestroyImmediate(seed);

            if (!check.Solved)
            {
                status = PuzzleBakeStatus.Unverified;
                Object.DestroyImmediate(variant);
                return null;
            }

            if (!check.StartFails)
            {
                status = PuzzleBakeStatus.StartNotFailed;
                Object.DestroyImmediate(variant);
                return null;
            }

            status = PuzzleBakeStatus.Baked;
            return variant;
        }

        private static PuzzlePlanetListAsset Planets(PuzzleSeed seed, SpaceShapingSettings settings)
        {
            PuzzleShapeAsset shape = PuzzleArtifactFactory.Shape(seed, settings);
            PuzzlePlanetListAsset planets = PuzzleArtifactFactory.Planets(shape);
            Object.DestroyImmediate(shape);
            return planets;
        }

        private static void WireScene(PuzzleData[] puzzles)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            SpaceShapingController state = FindState(scene);
            if (state == null)
            {
                Debug.LogError("Test set: no SpaceShapingState in the POC scene.");
                return;
            }

            SerializedObject serialized = new SerializedObject(state);
            SerializedProperty array = serialized.FindProperty("puzzles");
            array.arraySize = puzzles.Length;
            for (int i = 0; i < puzzles.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = puzzles[i];

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EnsureNextButton();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static SpaceShapingController FindState(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                SpaceShapingController state = roots[i].GetComponentInChildren<SpaceShapingController>(true);
                if (state != null)
                    return state;
            }

            return null;
        }

        private static void EnsureNextButton()
        {
            if (Object.FindAnyObjectByType<PuzzleLabNextButton>() != null)
                return;

            PuzzleLabResetButton reset = Object.FindAnyObjectByType<PuzzleLabResetButton>();
            if (reset == null)
            {
                Debug.LogWarning("Test set: no ResetButton to duplicate for Next.");
                return;
            }

            PuzzleLabPanel panel = PanelOf(reset);
            GameObject next = Object.Instantiate(reset.gameObject, reset.transform.parent);
            next.name = "NextButton";
            next.SetActive(false);
            next.transform.localPosition = new Vector3(0f, -0.05f, 0f);

            Object.DestroyImmediate(next.GetComponent<PuzzleLabResetButton>());
            PuzzleLabNextButton button = next.AddComponent<PuzzleLabNextButton>();
            SerializedObject serialized = new SerializedObject(button);
            serialized.FindProperty("panel").objectReferenceValue = panel;
            serialized.FindProperty("grabbable").objectReferenceValue = next.GetComponent<Grabbable>();
            serialized.FindProperty("face").objectReferenceValue = FaceOf(next);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            TMP_Text label = next.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = "NEXT";

            next.SetActive(true);
        }

        private static PuzzleLabPanel PanelOf(PuzzleLabResetButton reset)
        {
            SerializedObject serialized = new SerializedObject(reset);
            return serialized.FindProperty("panel").objectReferenceValue as PuzzleLabPanel;
        }

        private static Renderer FaceOf(GameObject button)
        {
            Transform face = button.transform.Find("Face");
            if (face == null)
            {
                Debug.LogWarning("Test set: Next button has no Face child.");
                return null;
            }

            return face.GetComponent<Renderer>();
        }
    }
}
