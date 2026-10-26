using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.EditorTools
{
    [CustomEditor(typeof(SpaceShapingState))]
    public class SpaceShapingStateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Preview field mesh"))
                ((SpaceShapingState)target).PreviewField();
        }
    }
}
