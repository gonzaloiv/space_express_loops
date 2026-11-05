using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection.EditorTools
{
    [CustomEditor(typeof(PuzzleMapView))]
    public class PuzzleMapViewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PuzzleMapView view = (PuzzleMapView)target;
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Swipe simulation", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Previous"))
                        view.SimulateSwipe(-1);
                    if (GUILayout.Button("Next"))
                        view.SimulateSwipe(1);
                }

                if (GUILayout.Button(view.ShowAll ? "Show Faded" : "Show All"))
                    view.SetShowAll(!view.ShowAll);
            }

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("Enter Play mode to simulate swipes.", MessageType.Info);
        }
    }
}
