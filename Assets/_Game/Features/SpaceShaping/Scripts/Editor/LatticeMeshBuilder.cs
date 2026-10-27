using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.EditorTools
{
    public static class LatticeMeshBuilder
    {
        private const int SamplesPerLine = 32;
        private static readonly Vector3 Min = new(-6f, -6f, -6f);
        private static readonly Vector3 Max = new(6f, 6f, 6f);
        private const string AssetPath = "Assets/_Game/Features/SpaceShaping/Models/LatticeMesh.asset";

        [MenuItem("DigitalLove/Space Shaping/Build Lattice Mesh")]
        public static void Build()
        {
            SpacetimeMeshView view = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<SpacetimeMeshView>()
                : null;
            if (view == null)
                view = UnityEngine.Object.FindFirstObjectByType<SpacetimeMeshView>();

            if (view == null)
            {
                Debug.LogError("[LatticeMeshBuilder] No SpacetimeMeshView in the open scene.");
                return;
            }

            Build(view.GridCount, view.Sides, view.Radius, view.GetComponent<MeshFilter>());
        }

        public static void Build(int gridCount, int sides, float radius, MeshFilter meshFilter)
        {
            string path = ResolvePath(meshFilter);
            EnsureFolder();

            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh();
                AssetDatabase.CreateAsset(mesh, path);
            }

            FillMesh(mesh, gridCount, SamplesPerLine, sides, Min, Max, radius);
            EditorUtility.SetDirty(mesh);
            AssetDatabase.SaveAssets();

            if (meshFilter != null)
                meshFilter.sharedMesh = mesh;

            Debug.Log($"[LatticeMeshBuilder] Built lattice mesh ({mesh.vertexCount} verts) at {path}");
        }

        public static Mesh BuildMesh(int n, int samples, int sides, Vector3 min, Vector3 max, float radius)
        {
            Mesh mesh = new();
            FillMesh(mesh, n, samples, sides, min, max, radius);
            return mesh;
        }

        public static void FillMesh(Mesh mesh, int n, int samples, int sides, Vector3 min, Vector3 max, float radius)
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

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        private static string ResolvePath(MeshFilter meshFilter)
        {
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                string path = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            return AssetPath;
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
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Features/SpaceShaping/Models"))
                AssetDatabase.CreateFolder("Assets/_Game/Features/SpaceShaping", "Models");
        }
    }
}
