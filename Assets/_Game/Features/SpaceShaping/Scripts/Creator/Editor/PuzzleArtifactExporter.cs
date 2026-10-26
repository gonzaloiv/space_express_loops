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
