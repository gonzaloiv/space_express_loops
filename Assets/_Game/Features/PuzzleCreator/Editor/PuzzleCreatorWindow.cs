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
        [SerializeField] private int variants = 1;
        [SerializeField] private float windowTightness = 1f;

        private PuzzleData[] generated;
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
            seed = (PuzzleSeed)EditorGUILayout.ObjectField("Seed", seed, typeof(PuzzleSeed), false);
            settings = (SpaceShapingSettings)EditorGUILayout.ObjectField(
                "Settings", settings, typeof(SpaceShapingSettings), false);
            shape = (PuzzleShapeAsset)EditorGUILayout.ObjectField(
                "Shape", shape, typeof(PuzzleShapeAsset), false);
            planets = (PuzzlePlanetListAsset)EditorGUILayout.ObjectField(
                "Planets", planets, typeof(PuzzlePlanetListAsset), false);
            variants = EditorGUILayout.IntSlider("Variants", variants, 1, 8);
            windowTightness = EditorGUILayout.FloatField("Window tightness (s)", windowTightness);
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
            bool ready = planets != null && settings != null;
            using (new EditorGUI.DisabledScope(!ready))
            {
                if (GUILayout.Button("Generate"))
                    Generate();
            }

            using (new EditorGUI.DisabledScope(seed == null || settings == null))
            {
                if (GUILayout.Button($"Find working seed (from {CurrentSeed()})"))
                    FindSeed();
            }

            using (new EditorGUI.DisabledScope(!ready))
            {
                if (GUILayout.Button("Save variants"))
                    Save();
            }
        }

        private int CurrentSeed() => seed != null ? seed.seed : 0;

        private void Generate()
        {
            generated = PuzzleFieldBake.BakeVariants(
                planets, PuzzleSimSettings.From(settings), windowTightness, variants, out status);
            Repaint();
        }

        private void FindSeed()
        {
            PuzzleSeedScanResult result = PuzzleSeedScanner.Find(
                seed, settings, windowTightness, variants, SeedAttempts);

            status = result.Status;
            if (!result.Found)
            {
                generated = null;
                Debug.LogWarning(
                    $"Puzzle creator: no working seed in {SeedAttempts} from {CurrentSeed()} ({result.Status}).");
                return;
            }

            seed.seed = result.Seed;
            EditorUtility.SetDirty(seed);
            shape = PuzzleArtifactExporter.ExportShape(seed, settings);
            planets = PuzzleArtifactExporter.ExportPlanets(shape);
            generated = result.Variants;
            Debug.Log($"Puzzle creator: seed {result.Seed} baked {generated.Length} variant(s).");
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

            PuzzleArtifactExporter.SaveVariants(generated, planets.name);
            Debug.Log($"Puzzle creator: saved {generated.Length} verified variant(s).");
        }

        private bool HasGenerated() =>
            generated != null && generated.Length > 0 && generated[0] != null;

        private void DrawPreview()
        {
            if (!HasGenerated())
            {
                MessageType type = status == PuzzleBakeStatus.Baked ? MessageType.Info : MessageType.Warning;
                EditorGUILayout.HelpBox(Message(status), type);
                return;
            }

            PuzzleData puzzle = generated[0];
            PuzzleVerification run = PuzzleVerifier.Verify(puzzle, PuzzleSimSettings.From(settings));

            Rect area = GUILayoutUtility.GetRect(0f, PreviewHeight, GUILayout.ExpandWidth(true));
            PuzzlePreviewDraw.Draw(area, puzzle, run);

            EditorGUILayout.LabelField("Start", run.StartOutcome.ToString());
            EditorGUILayout.LabelField("Solution", $"{run.SolvedOutcome} at {run.ShipTime:F2} s");
            EditorGUILayout.LabelField("Window", $"[{puzzle.tmin:F2}, {puzzle.tmax:F2}]");
            EditorGUILayout.LabelField(
                "Entropy", $"{run.EntropySpent:F2} / {puzzle.entropyBudget:F2}  ({Stars(run, puzzle)}★)");
            EditorGUILayout.LabelField("Variants", $"{generated.Length}");
        }

        private static string Message(PuzzleBakeStatus status)
        {
            if (status == PuzzleBakeStatus.StartNotMiss)
                return "Rejected: the bare start does not near-miss — it arrives or crashes into a planet. "
                    + "Raise the seed's distance (the field needs room to spread) or lower planetCount, "
                    + "or use Find working seed.";

            if (status == PuzzleBakeStatus.NoSolution)
                return "Rejected: no scale arrives. The field is too dense, or the run is too long for the launch speed.";

            if (status == PuzzleBakeStatus.Unverified)
                return "A solution was found but it did not verify inside the window/budget.";

            return "Set the seed + settings, export the shape and planets, then Generate.";
        }

        private static int Stars(PuzzleVerification run, PuzzleData puzzle)
        {
            float fraction = puzzle.entropyBudget > 0f ? run.EntropySpent / puzzle.entropyBudget : 0f;
            if (fraction <= 1f / 3f)
                return 3;

            return fraction <= 2f / 3f ? 2 : 1;
        }
    }
}
