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

            SpaceShapingState state = (SpaceShapingState)target;

            if (GUILayout.Button("Preview field mesh"))
                state.PreviewField();

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Apply solved cube (resets)"))
                {
                    float scale = PuzzleDebugSolution.Find(state.BuildSetup(), state.Zone);
                    bool applied = state.ApplySolvedScale(scale);
                    Debug.Log(applied
                        ? $"[SpaceShaping] Applied solved cube at scale {scale:0.0000}."
                        : "[SpaceShaping] No solution found in the reachable scale band.");
                }

                if (GUILayout.Button("Reset puzzle"))
                    state.ResetPuzzle();
            }

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("Enter Play mode to apply the solved cube.", MessageType.Info);
        }
    }
}
