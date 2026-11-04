using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    public static class PuzzleArtifactExporter
    {
        private const string SpaceShapingFolder = "Assets/_Game/Features/PuzzleCreator";
        private const string GeneratedName = "Resources";
        private const string GeneratedFolder = SpaceShapingFolder + "/" + GeneratedName;

        public static PuzzleShapeAsset ExportShape(PuzzleSeed seed, SpaceShapingSettings settings) =>
            Write(PuzzleArtifactFactory.Shape(seed, settings), $"{seed.name}_Shape");

        public static PuzzlePlanetListAsset ExportPlanets(PuzzleShapeAsset shape) =>
            Write(PuzzleArtifactFactory.Planets(shape), $"{shape.name}_Planets");

        public static void SaveVariant(PuzzleData variant, string name) =>
            SaveVariant(variant, name, GeneratedFolder);

        public static void SaveVariant(PuzzleData variant, string name, string folder) =>
            Write(variant, $"{name}_Puzzle", folder);

        private static T Write<T>(T asset, string name) where T : ScriptableObject =>
            Write(asset, name, GeneratedFolder);

        private static T Write<T>(T asset, string name, string folder) where T : ScriptableObject
        {
            EnsureFolder(folder);
            string path = $"{folder}/{name}.asset";
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                return asset;
            }

            if (existing != asset)
            {
                EditorUtility.CopySerialized(asset, existing);
                existing.name = name;
                UnityEngine.Object.DestroyImmediate(asset);
            }

            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            return existing;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            string parent = System.IO.Path.GetDirectoryName(folder)?.Replace('\\', '/');
            string leaf = System.IO.Path.GetFileName(folder);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf))
                return;

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
