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
                if (GUILayout.Button(view.ShowAll ? "Show Faded" : "Show All"))
                    view.SetShowAll(!view.ShowAll);
            }
        }
    }
}
