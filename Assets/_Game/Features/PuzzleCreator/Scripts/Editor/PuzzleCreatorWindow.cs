using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    public class PuzzleCreatorWindow : EditorWindow
    {
        private const float PreviewHeight = 240f;
        private const int SeedAttempts = 64;

        [SerializeField] private PuzzleSeed seed;
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private PuzzleShapeAsset shape;
        [SerializeField] private PuzzlePlanetListAsset planets;

        private PuzzleData generated;
        private PuzzleZone[] generatedSolution;
        private PuzzleBakeStatus status;
        private Vector2 scroll;

        [MenuItem("DigitalLove/Space Shaping/Puzzle Creator")]
        private static void Open() => GetWindow<PuzzleCreatorWindow>("Puzzle Creator");

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawInputs();
            DrawExport();
            DrawBake();
            DrawPreview();
            EditorGUILayout.EndScrollView();
        }

        private void DrawInputs()
        {
            seed = (PuzzleSeed)EditorGUILayout.ObjectField(
                "Seed asset", seed, typeof(PuzzleSeed), false);
            settings = (SpaceShapingSettings)EditorGUILayout.ObjectField(
                "Settings", settings, typeof(SpaceShapingSettings), false);
            shape = (PuzzleShapeAsset)EditorGUILayout.ObjectField(
                "Shape", shape, typeof(PuzzleShapeAsset), false);
            planets = (PuzzlePlanetListAsset)EditorGUILayout.ObjectField(
                "Planets", planets, typeof(PuzzlePlanetListAsset), false);
            DrawBrief();
        }

        private void DrawBrief()
        {
            if (seed == null)
            {
                EditorGUILayout.LabelField("Brief", "assign a seed");
                return;
            }

            EditorGUILayout.LabelField("Brief", BriefText(seed));
            EditorGUILayout.HelpBox("Edit the seed asset to change the brief.", MessageType.None);
        }

        private void DrawExport()
        {
            using (new EditorGUI.DisabledScope(seed == null || settings == null))
            {
                if (GUILayout.Button("Export shape"))
                    shape = PuzzleArtifactExporter.ExportShape(seed, settings);
            }

            using (new EditorGUI.DisabledScope(shape == null))
            {
                if (GUILayout.Button("Export planets"))
                    planets = PuzzleArtifactExporter.ExportPlanets(shape);
            }
        }

        private void DrawBake()
        {
            using (new EditorGUI.DisabledScope(seed == null || settings == null))
            {
                if (GUILayout.Button("Generate (rebuild)"))
                    Generate();

                if (GUILayout.Button($"Find working seed (from {CurrentSeed()})"))
                    FindSeed();

                if (GUILayout.Button("Save level"))
                    Save();
            }
        }

        private int CurrentSeed() => seed != null ? seed.seed : 0;

        private void Generate()
        {
            shape = PuzzleArtifactExporter.ExportShape(seed, settings);
            planets = PuzzleArtifactExporter.ExportPlanets(shape);
            generated = PuzzleFieldBake.Bake(
                planets, PuzzleSimSettings.From(settings), PuzzleBrief.From(seed, settings), out status,
                out generatedSolution);
            Repaint();
        }

        private void FindSeed()
        {
            PuzzleSeedScanResult result = PuzzleSeedScanner.Find(seed, settings, SeedAttempts);

            status = result.Status;
            if (!result.Found)
            {
                generated = null;
                generatedSolution = null;
                Debug.LogWarning(
                    $"Puzzle creator: no working seed in {SeedAttempts} from {CurrentSeed()} ({result.Status}).");
                return;
            }

            seed.seed = result.Seed;
            EditorUtility.SetDirty(seed);
            shape = PuzzleArtifactExporter.ExportShape(seed, settings);
            planets = PuzzleArtifactExporter.ExportPlanets(shape);
            generated = result.Puzzle;
            generatedSolution = result.Solution;
            Debug.Log($"Puzzle creator: seed {result.Seed} baked a verified level "
                + $"({result.Puzzle.moves} step(s), {result.Routes} route(s)).");
            Repaint();
        }

        private void Save()
        {
            if (!HasGenerated())
                Generate();

            if (!HasGenerated())
            {
                Debug.LogWarning($"Puzzle creator: nothing to save ({status}).");
                return;
            }

            PuzzleArtifactExporter.SaveVariant(generated, planets.name);
            Debug.Log("Puzzle creator: saved the verified level.");
        }

        private bool HasGenerated() => generated != null;

        private void DrawPreview()
        {
            if (!HasGenerated())
            {
                MessageType type = status == PuzzleBakeStatus.Baked ? MessageType.Info : MessageType.Warning;
                EditorGUILayout.HelpBox(Message(status), type);
                return;
            }

            PuzzleData puzzle = generated;
            PuzzleVerification run = PuzzleVerifier.Verify(
                puzzle, generatedSolution, PuzzleSimSettings.From(settings));

            Rect area = GUILayoutUtility.GetRect(0f, PreviewHeight, GUILayout.ExpandWidth(true));
            PuzzlePreviewDraw.Draw(area, puzzle, run);

            EditorGUILayout.LabelField("Brief", BriefText(puzzle));
            EditorGUILayout.LabelField(
                "Start", $"{run.StartOutcome} (margin {run.StartMargin:F3} m)");
            EditorGUILayout.LabelField("Solution", run.SolvedOutcome.ToString());
            EditorGUILayout.LabelField(
                "Entropy", $"{run.EntropySpent:F2} / {puzzle.entropyBudget:F2}  ({PuzzleGrade.Stars(run.EntropySpent, puzzle.entropyBudget)}★)");
            EditorGUILayout.LabelField("Cubes", FramesText(puzzle));
            EditorGUILayout.LabelField("Steps", puzzle.moves.ToString());
            EditorGUILayout.LabelField("Routes", puzzle.routes.ToString());
        }

        private static string FramesText(PuzzleData puzzle)
        {
            if (puzzle.frames == null || puzzle.frames.Length == 0)
                return "none";

            float min = puzzle.frames[0].size;
            float max = puzzle.frames[0].size;
            for (int i = 1; i < puzzle.frames.Length; i++)
            {
                min = Mathf.Min(min, puzzle.frames[i].size);
                max = Mathf.Max(max, puzzle.frames[i].size);
            }

            return Mathf.Approximately(min, max)
                ? $"{puzzle.frames.Length} × {min:F2} m"
                : $"{puzzle.frames.Length} · {min:F2}–{max:F2} m";
        }

        private static string BriefText(PuzzleSeed value) =>
            BriefText(value.translatable, value.moves, value.resizable)
            + $" · {value.fieldShape}";

        private static string BriefText(PuzzleData value) =>
            BriefText(value.translatable, value.moves, value.resizable);

        private static string BriefText(bool translatable, int moves, bool resizable) =>
            $"{(translatable ? "movable" : "fixed")} · {Mathf.Max(1, moves)} zone(s) · "
            + $"{(resizable ? "resizable" : "fixed size")}";

        private static string Message(PuzzleBakeStatus status)
        {
            if (status == PuzzleBakeStatus.UnsupportedBrief)
                return "Rejected: this creator supports 1–3 zones.";

            if (status == PuzzleBakeStatus.StartNotFailed)
                return "Rejected: the bare start already arrives. The level needs a failing start "
                    + "(a crash or a miss); use Find working seed.";

            if (status == PuzzleBakeStatus.FewerZones)
                return "Rejected: a solution with fewer zones still arrives at the same cube size, so this "
                    + "seed does not teach the extra zone. Use Find working seed.";

            if (status == PuzzleBakeStatus.SingleRoute)
                return "Rejected: a movable level needs at least two routes so the player can choose. "
                    + "Use Find working seed.";

            if (status == PuzzleBakeStatus.NoSolution)
                return "Rejected: no zone bends the bare run onto the destination. The field may be too "
                    + "dense. Use Find working seed.";

            if (status == PuzzleBakeStatus.Unverified)
                return "A solution was found but it did not verify inside the budget.";

            return "Set the seed + settings, then Generate (it rebuilds the shape and planets first).";
        }
    }
}
