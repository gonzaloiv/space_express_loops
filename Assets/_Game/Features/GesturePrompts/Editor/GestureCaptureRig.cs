using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DigitalLove.Game.GesturePrompts.Editor
{
    // A throwaway preview scene with Meta's two OpenXR hands, a wire cube and an orthographic
    // camera. It renders masks only (white on black); LineArt turns them into the final frames.
    public sealed class GestureCaptureRig : IDisposable
    {
        private const string Package = "Packages/com.meta.xr.sdk.interaction/Runtime/";
        private const float BarThickness = 0.022f;
        private static readonly Vector3 CubeTilt = new(18f, -28f, 0f);

        private readonly Scene scene;
        private readonly Material white;
        private readonly HandPoser left;
        private readonly HandPoser right;
        private readonly Transform cube;
        private readonly MeterRig meter;
        private readonly Renderer[] bars = new Renderer[12];
        private readonly Camera camera;
        private readonly RenderTexture target;
        private readonly Texture2D readback;

        public GestureCaptureRig(int resolution, Quaternion leftRotation, Quaternion rightRotation, float frameHeight, float frameDrop)
        {
            Resolution = resolution;
            scene = EditorSceneManager.NewPreviewScene();
            white = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { color = Color.white };
            left = SpawnHand("OpenXRLeftHand", "OpenXR_HandPinch_Left", leftRotation);
            right = SpawnHand("OpenXRRightHand", "OpenXR_HandPinch_Right", rightRotation);
            Unit = left.Length;
            cube = BuildCube();
            meter = new MeterRig(scene, white, Unit);
            camera = BuildCamera(frameHeight, frameDrop);
            target = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            camera.targetTexture = target;
            readback = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        }

        public int Resolution { get; }

        public float Unit { get; }

        public bool[] RenderHands(HandFrame frame)
        {
            Place(frame);
            ShowHands(frame);
            SetBars(false);
            meter.HideAll();
            return Capture();
        }

        public bool[] RenderCube(HandFrame frame)
        {
            Place(frame);
            left.SetVisible(false);
            right.SetVisible(false);
            SetBars(frame.CubeSize > 0f);
            meter.HideAll();
            return Capture();
        }

        public bool[] RenderMeter(HandFrame frame)
        {
            Place(frame);
            left.SetVisible(false);
            right.SetVisible(false);
            SetBars(false);
            meter.ShowMeter(frame.Meter);
            meter.ShowStar(false);
            return Capture();
        }

        public bool[] RenderStar(HandFrame frame)
        {
            Place(frame);
            left.SetVisible(false);
            right.SetVisible(false);
            SetBars(false);
            meter.ShowMeter(null);
            meter.ShowStar(frame.Meter.HasValue && frame.Meter.Value.StarAlpha > 0f);
            return Capture();
        }

        private void Place(HandFrame frame)
        {
            if (frame.Left.HasValue)
                left.Pose(frame.Grip, World(frame.Left.Value));
            if (frame.Right.HasValue)
                right.Pose(frame.Grip, World(frame.Right.Value));

            cube.position = World(frame.CubeCentre);
            cube.localScale = Vector3.one * Mathf.Max(frame.CubeSize * Unit, 0.0001f);
        }

        private void ShowHands(HandFrame frame)
        {
            left.SetVisible(frame.Left.HasValue);
            right.SetVisible(frame.Right.HasValue);
        }

        private void SetBars(bool visible)
        {
            foreach (Renderer bar in bars)
                bar.enabled = visible;
        }

        private Vector3 World(Vector2 hands) => new(hands.x * Unit, hands.y * Unit, 0f);

        private bool[] Capture()
        {
            var request = new RenderPipeline.StandardRequest { destination = target };
            if (RenderPipeline.SupportsRenderRequest(camera, request))
                RenderPipeline.SubmitRenderRequest(camera, request);
            else
                camera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, Resolution, Resolution), 0, 0);
            RenderTexture.active = previous;

            Color32[] pixels = readback.GetPixels32();
            bool[] mask = new bool[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
                mask[i] = pixels[i].r > 127;
            return mask;
        }

        private HandPoser SpawnHand(string mesh, string clip, Quaternion rotation)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Package + "Meshes/" + mesh + ".fbx");
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = white;

            var poser = new HandPoser(
                instance, AssetDatabase.LoadAssetAtPath<AnimationClip>(Package + "Animations/Hands/" + clip + ".anim"));
            instance.transform.rotation = rotation;
            return poser;
        }

        private Transform BuildCube()
        {
            var root = new GameObject("Cube").transform;
            SceneManager.MoveGameObjectToScene(root.gameObject, scene);
            root.rotation = Quaternion.Euler(CubeTilt);
            int index = 0;
            for (int axis = 0; axis < 3; axis++)
            {
                for (int corner = 0; corner < 4; corner++)
                    bars[index++] = BuildBar(root, axis, corner);
            }

            return root;
        }

        private Renderer BuildBar(Transform root, int axis, int corner)
        {
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.DestroyImmediate(bar.GetComponent<Collider>());
            SceneManager.MoveGameObjectToScene(bar, scene);
            bar.transform.SetParent(root, false);
            Vector3 offset = Vector3.zero;
            Vector3 scale = Vector3.one * BarThickness;
            int a = (axis + 1) % 3;
            int b = (axis + 2) % 3;
            offset[a] = (corner & 1) == 0 ? -0.5f : 0.5f;
            offset[b] = (corner & 2) == 0 ? -0.5f : 0.5f;
            scale[axis] = 1f + BarThickness;
            bar.transform.localPosition = offset;
            bar.transform.localScale = scale;
            Renderer renderer = bar.GetComponent<Renderer>();
            renderer.sharedMaterial = white;
            return renderer;
        }

        private Camera BuildCamera(float frameHeight, float frameDrop)
        {
            var go = new GameObject("Capture Camera");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.position = new Vector3(0f, -frameDrop * Unit, -20f * Unit);
            Camera cam = go.AddComponent<Camera>();
            cam.scene = scene;
            cam.orthographic = true;
            cam.orthographicSize = frameHeight * Unit;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.clear;
            cam.nearClipPlane = 0.1f * Unit;
            cam.farClipPlane = 60f * Unit;
            cam.cameraType = CameraType.Game;
            return cam;
        }

        public void Dispose()
        {
            camera.targetTexture = null;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(white);
            meter.Dispose();
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
