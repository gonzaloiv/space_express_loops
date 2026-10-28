using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    [CustomEditor(typeof(PuzzleSeed))]
    public sealed class PuzzleSeedEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Author this seed in DigitalLove > Space Shaping > Puzzle Creator.",
                MessageType.Info);

            if (GUILayout.Button("Open Puzzle Creator"))
                GetWindow<PuzzleCreatorWindow>("Puzzle Creator");
        }
    }
}
