using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.EditorTools
{
    public static class RangeRingMeshBuilder
    {
        private const int Segments = 48;
        private const int Sides = 4;
        private const float RingRadius = 0.5f;
        private const float TubeRadius = 0.006f;
        private const string ModelsFolder = "Assets/_Game/Features/SpaceShaping/Models";
        private const string AssetPath = ModelsFolder + "/RangeRings.asset";

        [MenuItem("DigitalLove/Space Shaping/Build Range Rings")]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(ModelsFolder))
                AssetDatabase.CreateFolder("Assets/_Game/Features/SpaceShaping", "Models");

            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(AssetPath);
            if (mesh == null)
            {
                mesh = new Mesh();
                AssetDatabase.CreateAsset(mesh, AssetPath);
            }

            FillMesh(mesh);
            EditorUtility.SetDirty(mesh);
            AssetDatabase.SaveAssets();
            Debug.Log($"[RangeRingMeshBuilder] Built range rings ({mesh.vertexCount} verts) at {AssetPath}");
        }

        public static Mesh BuildMesh()
        {
            Mesh mesh = new();
            FillMesh(mesh);
            return mesh;
        }

        private static void FillMesh(Mesh mesh)
        {
            List<Vector3> vertices = new();
            List<Vector3> normals = new();
            List<int> triangles = new();
            AddRing(vertices, normals, triangles, Vector3.forward);
            AddRing(vertices, normals, triangles, Vector3.up);
            AddRing(vertices, normals, triangles, Vector3.right);

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        private static void AddRing(
            List<Vector3> vertices, List<Vector3> normals, List<int> triangles, Vector3 axis)
        {
            Vector3 u = Vector3.Cross(axis, Vector3.up);
            if (u.sqrMagnitude < 0.001f)
                u = Vector3.Cross(axis, Vector3.right);
            u.Normalize();
            Vector3 v = Vector3.Cross(axis, u).normalized;

            int start = vertices.Count;
            for (int s = 0; s < Segments; s++)
            {
                float a = 2f * Mathf.PI * s / Segments;
                Vector3 radial = (u * Mathf.Cos(a) + v * Mathf.Sin(a)).normalized;
                for (int side = 0; side < Sides; side++)
                {
                    float b = 2f * Mathf.PI * side / Sides;
                    Vector3 offset = radial * (TubeRadius * Mathf.Cos(b)) + axis * (TubeRadius * Mathf.Sin(b));
                    vertices.Add(radial * RingRadius + offset);
                    normals.Add(offset.normalized);
                }
            }

            for (int s = 0; s < Segments; s++)
            {
                int next = (s + 1) % Segments;
                for (int side = 0; side < Sides; side++)
                {
                    int nextSide = (side + 1) % Sides;
                    int a0 = start + s * Sides + side;
                    int b0 = start + s * Sides + nextSide;
                    int c0 = start + next * Sides + side;
                    int d0 = start + next * Sides + nextSide;
                    triangles.Add(a0); triangles.Add(c0); triangles.Add(b0);
                    triangles.Add(b0); triangles.Add(c0); triangles.Add(d0);
                }
            }
        }
    }
}
