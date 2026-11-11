using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigitalLove.Game.GesturePrompts.Editor
{
    // The game's prompt rig: the module's hand preview plus the cube, meter and star the prompts show.
    public sealed class GestureCaptureRig : System.IDisposable
    {
        private const float BarThickness = 0.022f;
        private static readonly Vector3 CubeTilt = new(18f, -28f, 0f);

        private readonly DigitalLove.XR.GestureArt.Editor.GestureCaptureRig hands;
        private readonly Transform cube;
        private readonly MeterRig meter;
        private readonly Renderer[] bars = new Renderer[12];

        public GestureCaptureRig(
            int resolution, Quaternion leftRotation, Quaternion rightRotation,
            float frameHeight, float frameDrop)
        {
            hands = new DigitalLove.XR.GestureArt.Editor.GestureCaptureRig(
                resolution, leftRotation, rightRotation, frameHeight, frameDrop);
            cube = BuildCube();
            meter = new MeterRig(hands.Scene, hands.WhiteMaterial, Unit);
        }

        public int Resolution => hands.Resolution;

        public float Unit => hands.Unit;

        public bool[] RenderHands(HandFrame frame) => hands.RenderHands(frame.ToPose());

        public bool[] RenderCube(HandFrame frame) => hands.RenderOverlay(() =>
        {
            PlaceCube(frame);
            SetBars(frame.CubeSize > 0f);
            meter.HideAll();
        });

        public bool[] RenderMeter(HandFrame frame) => hands.RenderOverlay(() =>
        {
            SetBars(false);
            meter.ShowMeter(frame.Meter);
            meter.ShowStar(false);
        });

        public bool[] RenderStar(HandFrame frame) => hands.RenderOverlay(() =>
        {
            SetBars(false);
            meter.ShowMeter(null);
            meter.ShowStar(frame.Meter.HasValue && frame.Meter.Value.StarAlpha > 0f);
        });

        private void PlaceCube(HandFrame frame)
        {
            cube.position = World(frame.CubeCentre);
            cube.localScale = Vector3.one * Mathf.Max(frame.CubeSize * Unit, 0.0001f);
        }

        private void SetBars(bool visible)
        {
            foreach (Renderer bar in bars)
                bar.enabled = visible;
        }

        private Vector3 World(Vector2 handsPoint) =>
            new(handsPoint.x * Unit, handsPoint.y * Unit, 0f);

        private Transform BuildCube()
        {
            var root = new GameObject("Cube").transform;
            SceneManager.MoveGameObjectToScene(root.gameObject, hands.Scene);
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
            Object.DestroyImmediate(bar.GetComponent<Collider>());
            SceneManager.MoveGameObjectToScene(bar, hands.Scene);
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
            renderer.sharedMaterial = hands.WhiteMaterial;
            return renderer;
        }

        public void Dispose()
        {
            meter.Dispose();
            hands.Dispose();
        }
    }
}
