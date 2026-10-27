using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    public static class PuzzleArtifactExporter
    {
        private const string SpaceShapingFolder = "Assets/_Game/Features/SpaceShaping";
        private const string GeneratedName = "Generated";
        private const string GeneratedFolder = SpaceShapingFolder + "/" + GeneratedName;

        public static PuzzleShapeAsset ExportShape(PuzzleSeed seed, SpaceShapingSettings settings) =>
            Write(PuzzleArtifactFactory.Shape(seed, settings), $"{seed.name}_Shape");

        public static PuzzlePlanetListAsset ExportPlanets(PuzzleShapeAsset shape) =>
            Write(PuzzleArtifactFactory.Planets(shape), $"{shape.name}_Planets");

        public static void SaveVariants(PuzzleData[] variants, string name)
        {
            for (int i = 0; i < variants.Length; i++)
                Write(variants[i], $"{name}_Puzzle_{i + 1}");
        }

        private static T Write<T>(T asset, string name) where T : ScriptableObject
        {
            EnsureFolder();
            string path = $"{GeneratedFolder}/{name}.asset";
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

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(GeneratedFolder))
                AssetDatabase.CreateFolder(SpaceShapingFolder, GeneratedName);
        }
    }
}
