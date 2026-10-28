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

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Apply solved cube (resets)"))
                {
                    float scale = ((SpaceShapingState)target).DebugApplySolvedCube();
                    Debug.Log(scale > 0f
                        ? $"[SpaceShaping] Applied solved cube at scale {scale:0.0000}."
                        : "[SpaceShaping] No solution found in the reachable scale band.");
                }
            }

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("Enter Play mode to apply the solved cube.", MessageType.Info);
        }
    }
}
