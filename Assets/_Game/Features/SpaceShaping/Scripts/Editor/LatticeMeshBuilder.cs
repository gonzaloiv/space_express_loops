using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.EditorTools
{
    public static class LatticeMeshBuilder
    {
        private const int GridCount = 4;
        private const int SamplesPerLine = 32;
        private const int Sides = 6;
        private const float Radius = 0.05f;
        private static readonly Vector3 Min = new(-6f, -4f, -3f);
        private static readonly Vector3 Max = new(6f, 4f, 3f);
        private const string AssetPath = "Assets/_Game/Features/SpaceShaping/Meshes/LatticeMesh.asset";

        [MenuItem("DigitalLove/Space Shaping/Build Lattice Mesh")]
        public static void Build()
        {
            Mesh mesh = BuildMesh(GridCount, SamplesPerLine, Sides, Min, Max, Radius);
            EnsureFolder();
            if (AssetDatabase.LoadAssetAtPath<Mesh>(AssetPath) != null)
                AssetDatabase.DeleteAsset(AssetPath);
            AssetDatabase.CreateAsset(mesh, AssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[LatticeMeshBuilder] Built lattice mesh ({mesh.vertexCount} verts) at {AssetPath}");
        }

        public static Mesh BuildMesh(int n, int samples, int sides, Vector3 min, Vector3 max, float radius)
        {
            List<Vector3> vertices = new();
            List<Vector3> normals = new();
            List<int> triangles = new();

            int perAxis = n * n;
            for (int axis = 0; axis < 3; axis++)
            {
                for (int k = 0; k < perAxis; k++)
                {
                    int i = k / n;
                    int j = k % n;
                    AddTube(vertices, normals, triangles, axis, i, j, n, samples, sides, min, max, radius);
                }
            }

            Mesh mesh = new();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddTube(
            List<Vector3> vertices, List<Vector3> normals, List<int> triangles,
            int axis, int i, int j, int n, int samples, int sides, Vector3 min, Vector3 max, float radius)
        {
            int start = vertices.Count;
            for (int s = 0; s < samples; s++)
            {
                float nodeIndex = (float)s / (samples - 1) * (n - 1);
                Vector3 node = LatticeGrid.Node(axis, nodeIndex, i, j, n, min, max);
                for (int side = 0; side < sides; side++)
                {
                    Vector3 ring = LatticeGrid.RingOffset(axis, side, sides) * radius;
                    vertices.Add(node + ring);
                    normals.Add(ring.normalized);
                }
            }

            for (int s = 0; s < samples - 1; s++)
            {
                for (int side = 0; side < sides; side++)
                {
                    int nextSide = (side + 1) % sides;
                    int a = start + s * sides + side;
                    int b = start + s * sides + nextSide;
                    int c = start + (s + 1) * sides + side;
                    int d = start + (s + 1) * sides + nextSide;
                    triangles.Add(a); triangles.Add(c); triangles.Add(b);
                    triangles.Add(b); triangles.Add(c); triangles.Add(d);
                }
            }
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Features/SpaceShaping/Meshes"))
                AssetDatabase.CreateFolder("Assets/_Game/Features/SpaceShaping", "Meshes");
        }
    }
}
