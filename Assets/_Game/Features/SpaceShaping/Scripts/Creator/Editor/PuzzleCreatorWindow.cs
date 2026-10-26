using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    public class PuzzleCreatorWindow : EditorWindow
    {
        [SerializeField] private PuzzleSeed seed;
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private PuzzleShapeAsset shape;
        [SerializeField] private PuzzlePlanetListAsset planets;
        [SerializeField] private int variants = 1;
        [SerializeField] private float windowTightness = 1f;

        [MenuItem("DigitalLove/Space Shaping/Puzzle Creator")]
        private static void Open() => GetWindow<PuzzleCreatorWindow>("Puzzle Creator");

        private void OnGUI()
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

            DrawShape();
            DrawPlanets();
            DrawBake();
        }

        private void DrawShape()
        {
            using (new EditorGUI.DisabledScope(seed == null || settings == null))
            {
                if (GUILayout.Button("Export shape"))
                    shape = PuzzleArtifactExporter.ExportShape(seed, settings);
            }
        }

        private void DrawPlanets()
        {
            using (new EditorGUI.DisabledScope(shape == null))
            {
                if (GUILayout.Button("Export planets"))
                    planets = PuzzleArtifactExporter.ExportPlanets(shape);
            }
        }

        private void DrawBake()
        {
            using (new EditorGUI.DisabledScope(planets == null || settings == null))
            {
                if (GUILayout.Button("Bake variants"))
                    Bake();
            }
        }

        private void Bake()
        {
            PuzzleData[] baked = PuzzleArtifactExporter.ExportVariants(
                planets, settings, windowTightness, variants);
            Debug.Log($"Puzzle creator: baked {baked.Length} verified variant(s) from {planets.name}.");
        }
    }
}
