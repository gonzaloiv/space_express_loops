using System.IO;
using DigitalLove.XR.GestureArt;
using DigitalLove.XR.GestureArt.Editor;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.GesturePrompts.Editor
{
    // Renders the game's five prompt loops from Meta's own hand mesh and pinch clip, then assigns each
    // to a GestureLoop asset. The Exit prompt uses the module's shared menu loop. Re-run after tuning
    // a motion (GestureMotion) or the look (LineArt); the PNGs are overwritten in place.
    public static class GesturePromptArtCapture
    {
        private const string OutputRoot = "Assets/_Game/Features/GesturePrompts/Art";
        private const string ContentRoot = "Assets/_Game/Resources/GesturePrompts/";
        private const string LoopRoot = "Assets/_Game/Resources/GesturePrompts/Loops";
        private const int OutputSize = 512;
        private const int Supersample = 2;
        private const int LineWidth = 14;
        private const int FrameCount = 12;
        private const float FrameSecs = 0.1f;
        private const float FrameHeight = 1.05f;
        private const float FrameDrop = 0.12f;
        private const float Upright = 75f;

        // Side-on so the thumb–index ring of the pinch faces the viewer, then rolled upright with the
        // fingers leaning in, mirrored per hand.
        private static readonly Quaternion LeftRotation =
            Quaternion.AngleAxis(Upright, Vector3.forward) * Quaternion.Euler(-90f, 0f, 90f);
        private static readonly Quaternion RightRotation =
            Quaternion.AngleAxis(-Upright, Vector3.forward) * Quaternion.Euler(-90f, 0f, -90f);

        [MenuItem("Tools/Our Space/Render Gesture Prompt Art")]
        public static void RenderAll()
        {
            GestureArchive.RenderAll();
            int resolution = OutputSize * Supersample;
            using (var rig = new GestureCaptureRig(
                resolution, LeftRotation, RightRotation, FrameHeight, FrameDrop))
            {
                foreach (GestureMotion motion in GestureMotion.All)
                    WriteFrames(rig, motion);
            }

            AssetDatabase.Refresh();
            foreach (GestureMotion motion in GestureMotion.All)
                AssignLoop(motion);
            AssignExitLoop();
            AssetDatabase.SaveAssets();
        }

        private static void WriteFrames(GestureCaptureRig rig, GestureMotion motion)
        {
            Directory.CreateDirectory(Folder(motion));
            var texture = new Texture2D(OutputSize, OutputSize, TextureFormat.RGBA32, false);
            for (int i = 0; i < FrameCount; i++)
            {
                HandFrame frame = motion.At(i / (float)FrameCount);
                var masks = new PromptMasks(
                    rig.RenderHands(frame), rig.RenderCube(frame),
                    rig.RenderMeter(frame), rig.RenderStar(frame),
                    frame.Meter?.StarAlpha ?? 1f);
                Color32[] pixels = LineArt.Compose(
                    masks, rig.Resolution, LineWidth * Supersample / 2, Supersample);
                texture.SetPixels32(pixels);
                File.WriteAllBytes(FramePath(motion, i), texture.EncodeToPNG());
            }

            Object.DestroyImmediate(texture);
        }

        private static void AssignLoop(GestureMotion motion)
        {
            GestureLoop loop = LoadOrCreateLoop(motion.Name);
            Sprite[] frames = new Sprite[FrameCount];
            for (int i = 0; i < FrameCount; i++)
                frames[i] = ImportSprite(FramePath(motion, i));
            loop.Configure(frames, FrameSecs);
            EditorUtility.SetDirty(loop);

            var content = AssetDatabase.LoadAssetAtPath<GesturePromptContent>(
                ContentRoot + motion.ContentAsset + ".asset");
            var so = new SerializedObject(content);
            so.FindProperty("loop").objectReferenceValue = loop;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignExitLoop()
        {
            var content = AssetDatabase.LoadAssetAtPath<GesturePromptContent>(
                ContentRoot + "GesturePromptExit.asset");
            if (content == null)
                return;

            GestureLoop menu = AssetDatabase.LoadAssetAtPath<GestureLoop>(GestureArchive.MenuLoopPath);
            if (menu == null)
            {
                Debug.LogWarning("The gesture archive has no Menu loop; run Tools/Our Space/Render Gesture Art Archive.");
                return;
            }

            var so = new SerializedObject(content);
            so.FindProperty("loop").objectReferenceValue = menu;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GestureLoop LoadOrCreateLoop(string assetName)
        {
            Directory.CreateDirectory(LoopRoot);
            string path = LoopRoot + "/" + assetName + ".asset";
            GestureLoop loop = AssetDatabase.LoadAssetAtPath<GestureLoop>(path);
            if (loop != null)
                return loop;

            loop = ScriptableObject.CreateInstance<GestureLoop>();
            AssetDatabase.CreateAsset(loop, path);
            return loop;
        }

        private static Sprite ImportSprite(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static string Folder(GestureMotion motion) => OutputRoot + "/" + motion.Name;

        private static string FramePath(GestureMotion motion, int index) =>
            $"{Folder(motion)}/{motion.Name}_{index:00}.png";
    }
}
