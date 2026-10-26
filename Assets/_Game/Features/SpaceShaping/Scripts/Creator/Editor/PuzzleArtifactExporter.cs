using System.IO;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    public static class PuzzleArtifactExporter
    {
        private const string GeneratedFolder = "Assets/_Game/Features/SpaceShaping/Generated";

        public static PuzzleShapeAsset ExportShape(PuzzleSeed seed, SpaceShapingSettings settings) =>
            Write(PuzzleArtifactFactory.Shape(seed, settings), $"{seed.name}_Shape");

        public static PuzzlePlanetListAsset ExportPlanets(PuzzleShapeAsset shape) =>
            Write(PuzzleArtifactFactory.Planets(shape), $"{shape.name}_Planets");

        public static PuzzleData[] ExportVariants(
            PuzzlePlanetListAsset planets, SpaceShapingSettings settings, float windowTightness, int variants)
        {
            PuzzleData[] baked = PuzzleFieldBake.BakeVariants(
                planets, PuzzleSimSettings.From(settings), windowTightness, variants);
            for (int i = 0; i < baked.Length; i++)
                Write(baked[i], $"{planets.name}_Puzzle_{i + 1}");

            return baked;
        }

        private static T Write<T>(T asset, string name) where T : ScriptableObject
        {
            EnsureFolder();
            string path = $"{GeneratedFolder}/{name}.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static void EnsureFolder()
        {
            if (Directory.Exists(GeneratedFolder))
                return;

            Directory.CreateDirectory(GeneratedFolder);
            AssetDatabase.Refresh();
        }
    }
}
