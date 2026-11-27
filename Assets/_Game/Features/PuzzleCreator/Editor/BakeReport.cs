using System.IO;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    // Batch runs read their result from Logs/bake-<name>.txt, because the console is gone by then.
    public static class BakeReport
    {
        public static void Write(string name, string text)
        {
            string folder = Path.Combine(ProjectRoot(), "Logs");
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, $"bake-{name}.txt"), text);
        }

        private static string ProjectRoot() =>
            Path.GetDirectoryName(Application.dataPath) ?? Directory.GetCurrentDirectory();
    }
}
