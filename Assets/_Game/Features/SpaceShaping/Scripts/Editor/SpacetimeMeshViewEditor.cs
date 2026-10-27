using DigitalLove.Game.SpaceShaping;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.EditorTools
{
    [CustomEditor(typeof(SpacetimeMeshView))]
    public class SpacetimeMeshViewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Grid Count, Sides and Radius must match the baked mesh. Rebuild it from these fields.",
                MessageType.Info);

            if (GUILayout.Button("Build lattice mesh"))
                BuildFromFields();
        }

        private void BuildFromFields()
        {
            SerializedProperty gridCount = serializedObject.FindProperty("gridCount");
            SerializedProperty sides = serializedObject.FindProperty("sides");
            SerializedProperty radius = serializedObject.FindProperty("radius");
            SerializedProperty meshFilter = serializedObject.FindProperty("meshFilter");

            LatticeMeshBuilder.Build(
                gridCount.intValue,
                sides.intValue,
                radius.floatValue,
                meshFilter.objectReferenceValue as MeshFilter);
        }
    }
}
