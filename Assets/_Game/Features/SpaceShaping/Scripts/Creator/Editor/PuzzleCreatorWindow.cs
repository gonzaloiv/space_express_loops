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
        [SerializeField] private PuzzleData target;

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
            target = (PuzzleData)EditorGUILayout.ObjectField("Target puzzle", target, typeof(PuzzleData), false);

            DrawExportShape();
            DrawExportPlanets();
            DrawBake();
        }

        private void DrawExportShape()
        {
            using (new EditorGUI.DisabledScope(seed == null || settings == null))
            {
                if (GUILayout.Button("Export shape"))
                    shape = PuzzleArtifactExporter.ExportShape(seed, settings);
            }
        }

        private void DrawExportPlanets()
        {
            using (new EditorGUI.DisabledScope(shape == null))
            {
                if (GUILayout.Button("Export planets"))
                    planets = PuzzleArtifactExporter.ExportPlanets(shape);
            }
        }

        private void DrawBake()
        {
            using (new EditorGUI.DisabledScope(planets == null || target == null))
            {
                if (GUILayout.Button("Bake field"))
                    Bake();
            }
        }

        private void Bake()
        {
            PuzzleFieldBake.Apply(target, planets);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}
