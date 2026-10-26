using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.EditorTools
{
    public static class ZoneFrameMeshBuilder
    {
        private const int Sides = 4;
        private const float BarRadius = 0.014f;
        private const string ModelsFolder = "Assets/_Game/Features/SpaceShaping/Models";
        private const string AssetPath = ModelsFolder + "/ZoneFrame.asset";

        [MenuItem("DigitalLove/Space Shaping/Build Zone Frame")]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(ModelsFolder))
                AssetDatabase.CreateFolder("Assets/_Game/Features/SpaceShaping", "Models");

            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(AssetPath);
            Mesh mesh = existing != null ? existing : new Mesh();
            BuildInto(mesh);

            if (existing == null)
                AssetDatabase.CreateAsset(mesh, AssetPath);
            else
                EditorUtility.SetDirty(mesh);

            AssetDatabase.SaveAssets();
            Debug.Log($"[ZoneFrameMeshBuilder] Built zone frame ({mesh.vertexCount} verts) at {AssetPath}");
        }

        private static Mesh BuildInto(Mesh mesh)
        {
            List<Vector3> vertices = new();
            List<Vector3> normals = new();
            List<int> triangles = new();

            Vector3[] corners = new Vector3[8];
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = new Vector3(
                    (i & 1) == 0 ? -0.5f : 0.5f,
                    (i & 2) == 0 ? -0.5f : 0.5f,
                    (i & 4) == 0 ? -0.5f : 0.5f);
            }

            int[,] edges =
            {
                { 0, 1 }, { 2, 3 }, { 4, 5 }, { 6, 7 },
                { 0, 2 }, { 1, 3 }, { 4, 6 }, { 5, 7 },
                { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 }
            };

            for (int e = 0; e < edges.GetLength(0); e++)
                AddBar(vertices, normals, triangles, corners[edges[e, 0]], corners[edges[e, 1]]);

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddBar(
            List<Vector3> vertices, List<Vector3> normals, List<int> triangles, Vector3 a, Vector3 b)
        {
            Vector3 axis = (b - a).normalized;
            Vector3 u = Vector3.Cross(axis, Vector3.up);
            if (u.sqrMagnitude < 0.001f)
                u = Vector3.Cross(axis, Vector3.right);
            u.Normalize();
            Vector3 v = Vector3.Cross(axis, u).normalized;

            int start = vertices.Count;
            for (int end = 0; end < 2; end++)
            {
                Vector3 point = end == 0 ? a : b;
                for (int side = 0; side < Sides; side++)
                {
                    float angle = 2f * Mathf.PI * side / Sides;
                    Vector3 offset = u * (BarRadius * Mathf.Cos(angle)) + v * (BarRadius * Mathf.Sin(angle));
                    vertices.Add(point + offset);
                    normals.Add(offset.normalized);
                }
            }

            for (int side = 0; side < Sides; side++)
            {
                int next = (side + 1) % Sides;
                int a0 = start + side;
                int b0 = start + next;
                int c0 = start + Sides + side;
                int d0 = start + Sides + next;
                triangles.Add(a0); triangles.Add(c0); triangles.Add(b0);
                triangles.Add(b0); triangles.Add(c0); triangles.Add(d0);
            }
        }
    }
}
