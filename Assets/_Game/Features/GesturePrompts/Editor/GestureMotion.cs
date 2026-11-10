using UnityEngine;

namespace DigitalLove.Game.GesturePrompts.Editor
{
    // One gesture as a loop: hands open, pinch, do the motion, release. Positions are in hand
    // lengths around the camera centre, so the art does not depend on the mesh's units.
    public class GestureMotion
    {
        public delegate HandFrame Sampler(float grip, float move);

        public GestureMotion(string name, string contentAsset, Sampler sample)
        {
            Name = name;
            ContentAsset = contentAsset;
            Sample = sample;
        }

        public string Name { get; }

        public string ContentAsset { get; }

        public Sampler Sample { get; }

        public HandFrame At(float t) => Sample(Grip(t), Move(t));

        // Open → pinched (0–15%), held through the motion, released at the end.
        private static float Grip(float t)
        {
            if (t < 0.15f)
                return Mathf.SmoothStep(0f, 1f, t / 0.15f);
            if (t < 0.75f)
                return 1f;
            if (t < 0.9f)
                return Mathf.SmoothStep(1f, 0f, (t - 0.75f) / 0.15f);
            return 0f;
        }

        private static float Move(float t) => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, 0.75f, t));

        // The visible pinch ring sits above the tip bones' midpoint; lifting the cube keeps it inside.
        private static readonly Vector2 CubeLift = new(0f, 0.14f);

        // Scale and enter straddle the cube: the pinches start on its faces and pull out past them,
        // the comfortable pose (the point between the hands is what must be on the cube).
        public static readonly GestureMotion[] All =
        {
            new("scale", "GesturePromptScale", (grip, move) =>
                HandFrame.TwoHands(grip, 0.25f + 0.15f * move, CubeLift, 0.5f)),
            new("enter", "GesturePromptEnter", (grip, move) =>
                HandFrame.TwoHands(grip, 0.22f + 0.16f * move, CubeLift, 0.4f + 0.3f * move)),
            new("move", "GesturePromptMove", (grip, move) =>
                HandFrame.OneHand(grip, new Vector2(-0.5f + 0.45f * move, CubeLift.y), 0.55f)),
            new("exit", "GesturePromptExit", (grip, move) =>
                HandFrame.TwoHands(grip, 0.38f - 0.2f * move, Vector2.zero, 0f)),
        };
    }

    public readonly struct HandFrame
    {
        public HandFrame(float grip, Vector2? left, Vector2? right, Vector2 cubeCentre, float cubeSize)
        {
            Grip = grip;
            Left = left;
            Right = right;
            CubeCentre = cubeCentre;
            CubeSize = cubeSize;
        }

        public float Grip { get; }

        public Vector2? Left { get; }

        public Vector2? Right { get; }

        public Vector2 CubeCentre { get; }

        public float CubeSize { get; }

        public static HandFrame TwoHands(float grip, float halfSpread, Vector2 cube, float cubeSize) =>
            new(grip, new Vector2(-halfSpread, 0f), new Vector2(halfSpread, 0f), cube, cubeSize);

        // The right hand pinches the cube's right face and slides it.
        public static HandFrame OneHand(float grip, Vector2 cube, float cubeSize) =>
            new(grip, null, new Vector2(cube.x + cubeSize * 0.5f, 0f), cube, cubeSize);
    }
}
