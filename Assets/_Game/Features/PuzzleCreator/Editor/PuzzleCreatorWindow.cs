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
            EditorGUI.BeginChangeCheck();
            int seedValue = EditorGUILayout.IntField("Seed value", seed.seed);
            int planetCount = EditorGUILayout.IntField("Planet count", seed.planetCount);
            PuzzleFieldShape fieldShape =
                (PuzzleFieldShape)EditorGUILayout.EnumPopup("Field shape", seed.fieldShape);
            bool translatable = EditorGUILayout.Toggle("Translatable", seed.translatable);
            int moves = EditorGUILayout.IntSlider("Moves", seed.moves, 1, 3);
            bool resizable = EditorGUILayout.Toggle("Resizable", seed.resizable);
            bool rotate = EditorGUILayout.Toggle("Rotate", seed.rotate);
            float margin = EditorGUILayout.Slider("Start margin", seed.startMargin, 0.5f, 1.5f);
            bool offPath = EditorGUILayout.Toggle("Off-path clock", seed.offPath);
            float tightness = EditorGUILayout.Slider("Window tightness", seed.windowTightness, 0f, 1f);
            bool fixedLevel = moves > 1 && !translatable;
            Vector3 zoneA = seed.fixedZoneA;
            Vector3 zoneB = seed.fixedZoneB;
            float zoneSize = seed.fixedZoneSize;
            bool proof = seed.proof;
            if (fixedLevel)
            {
                EditorGUILayout.LabelField("Fixed two zones", EditorStyles.boldLabel);
                zoneA = EditorGUILayout.Vector3Field("Zone A", zoneA);
                zoneB = EditorGUILayout.Vector3Field("Zone B", zoneB);
                zoneSize = EditorGUILayout.FloatField("Zone size", zoneSize);
                proof = EditorGUILayout.Toggle("Proof (skip minimality)", proof);
                if (GUILayout.Button("Use two wells") && planets != null && planets.planets.Length >= 3)
                {
                    zoneA = planets.planets[0].position;
                    zoneB = planets.planets[2].position;
                }

                EditorGUILayout.HelpBox(
                    "Each fixed cube must sit on a field source. The bake searches the two scales.",
                    MessageType.Info);
            }

            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(seed, "Puzzle brief");
            seed.seed = Mathf.Max(0, seedValue);
            seed.planetCount = Mathf.Max(1, planetCount);
            seed.fieldShape = fieldShape;
            seed.translatable = translatable;
            seed.moves = moves;
            seed.resizable = resizable;
            seed.rotate = rotate;
            seed.startMargin = margin;
            seed.offPath = offPath;
            seed.windowTightness = tightness;
            seed.fixedZoneA = zoneA;
            seed.fixedZoneB = zoneB;
            seed.fixedZoneSize = zoneSize;
            seed.proof = fixedLevel && proof;
            EditorUtility.SetDirty(seed);
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

                bool fixedLevel = seed != null && seed.moves > 1 && !seed.translatable;
                using (new EditorGUI.DisabledScope(fixedLevel))
                {
                    if (GUILayout.Button($"Find working seed (from {CurrentSeed()})"))
                        FindSeed();
                }

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
                out generatedSolution, requireMinimal: !seed.proof);
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
            Debug.Log($"Puzzle creator: seed {result.Seed} baked a verified level.");
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
                "Start", $"{run.StartOutcome} at {run.Start.ShipTime:F3} s (margin {run.StartMargin:F3} m)");
            EditorGUILayout.LabelField("Solution", $"{run.SolvedOutcome} at {run.ShipTime:F3} s");
            EditorGUILayout.LabelField("Window", $"[{puzzle.tmin:F2}, {puzzle.tmax:F2}]");
            EditorGUILayout.LabelField(
                "Entropy", $"{run.EntropySpent:F2} / {puzzle.entropyBudget:F2}  ({Stars(run, puzzle)}★)");
            EditorGUILayout.LabelField("Cube", $"{puzzle.cubeSize:F2} m");
        }

        private static string BriefText(PuzzleSeed value) =>
            BriefText(value.translatable, value.moves, value.resizable, value.rotate)
            + $" · {value.fieldShape} · start {value.startMargin:F2}× · {(value.offPath ? "clock" : "path")}";

        private static string BriefText(PuzzleData value) =>
            BriefText(value.translatable, value.moves, value.resizable, value.rotate)
            + $" · launch {value.launchSpeed:F2} m/s";

        private static string BriefText(bool translatable, int moves, bool resizable, bool rotate) =>
            $"{(translatable ? "movable" : "fixed")} · {Mathf.Max(1, moves)} zone(s) · "
            + $"{(resizable ? "resizable" : "fixed size")} · {(rotate ? "rotate" : "no rotate")}";

        private static string Message(PuzzleBakeStatus status)
        {
            if (status == PuzzleBakeStatus.UnsupportedBrief)
                return "Rejected: this creator supports 1–3 zones and no rotate. Multi-zone can be "
                    + "translatable (moving cube) or fixed two-zone (authored zones); off-path levels "
                    + "stay single-zone; rotate must be off.";

            if (status == PuzzleBakeStatus.StartNotFailed)
                return "Rejected: the bare start already solves or crashes. The level needs a failing "
                    + "start (a miss, or an arrival outside the window); use Find working seed.";

            if (status == PuzzleBakeStatus.FewerZones)
                return "Rejected: a solution with fewer zones still arrives inside the window at the same "
                    + "cube size, so this seed does not teach the extra zone. Use Find working seed.";

            if (status == PuzzleBakeStatus.NoSolution)
                return "Rejected: no solution inside the window. For a clock level, the bare launch must "
                    + "arrive and one scale must move the clock clear of it; for a path level the field may "
                    + "be too dense or the window too tight. Use Find working seed.";

            if (status == PuzzleBakeStatus.Unverified)
                return "A solution was found but it did not verify inside the window/budget.";

            return "Set the seed + settings, then Generate (it rebuilds the shape and planets first).";
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
