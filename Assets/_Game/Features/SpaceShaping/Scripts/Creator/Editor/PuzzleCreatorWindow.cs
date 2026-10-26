using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    public class PuzzleCreatorWindow : EditorWindow
    {
        [SerializeField] private PuzzleSeed seed;
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private PuzzleData target;

        [MenuItem("DigitalLove/Space Shaping/Puzzle Creator")]
        private static void Open() => GetWindow<PuzzleCreatorWindow>("Puzzle Creator");

        private void OnGUI()
        {
            seed = (PuzzleSeed)EditorGUILayout.ObjectField("Seed", seed, typeof(PuzzleSeed), false);
            settings = (SpaceShapingSettings)EditorGUILayout.ObjectField(
                "Settings", settings, typeof(SpaceShapingSettings), false);
            target = (PuzzleData)EditorGUILayout.ObjectField("Target puzzle", target, typeof(PuzzleData), false);

            using (new EditorGUI.DisabledScope(seed == null || settings == null || target == null))
            {
                if (GUILayout.Button("Bake field from seed"))
                    Bake();
            }
        }

        private void Bake()
        {
            PuzzleFieldBake.Apply(target, seed, settings);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}
