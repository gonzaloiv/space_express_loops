using DigitalLove.Game.SpaceShaping;
using DigitalLove.XR.GestureArt;
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
            new("spend", "GesturePromptSpend", (grip, move) =>
                new HandFrame(
                    grip, new Vector2(-Spread(move), 0f), new Vector2(Spread(move), 0f), CubeLift, 0.5f)
                    .WithMeter(new MeterFrame(0.05f + 0.7f * move, 0f))),
            new("stars", "GesturePromptStars", (_, move) =>
            {
                float spend = 0.05f + 0.65f * move;
                float fadeEnd = (PuzzleGrade.ThreeStarUse + PuzzleGrade.TwoStarUse) * 0.5f;
                float star = Mathf.Lerp(
                    1f, 0.28f, Mathf.InverseLerp(PuzzleGrade.ThreeStarUse, fadeEnd, spend));
                return new HandFrame(0f, null, null, Vector2.zero, 0f)
                    .WithMeter(new MeterFrame(spend, star));
            }),
        };

        private static float Spread(float move) => 0.25f + 0.15f * move;
    }

    // The base square under the cube: how much has been spent from the front, and how bright the
    // star above it reads (0 keeps it out of the art entirely).
    public readonly struct MeterFrame
    {
        public MeterFrame(float spend, float starAlpha)
        {
            Spend = Mathf.Clamp01(spend);
            StarAlpha = Mathf.Clamp01(starAlpha);
        }

        public float Spend { get; }

        public float StarAlpha { get; }
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
            Meter = null;
        }

        private HandFrame(HandFrame source, MeterFrame meter)
        {
            Grip = source.Grip;
            Left = source.Left;
            Right = source.Right;
            CubeCentre = source.CubeCentre;
            CubeSize = source.CubeSize;
            Meter = meter;
        }

        public float Grip { get; }

        public Vector2? Left { get; }

        public Vector2? Right { get; }

        public Vector2 CubeCentre { get; }

        public float CubeSize { get; }

        public MeterFrame? Meter { get; }

        public HandFrame WithMeter(MeterFrame meter) => new(this, meter);

        public GesturePose ToPose() => new(Grip, Left, Right);

        public static HandFrame TwoHands(float grip, float halfSpread, Vector2 cube, float cubeSize) =>
            new(grip, new Vector2(-halfSpread, 0f), new Vector2(halfSpread, 0f), cube, cubeSize);

        // The right hand pinches the cube's right face and slides it.
        public static HandFrame OneHand(float grip, Vector2 cube, float cubeSize) =>
            new(grip, null, new Vector2(cube.x + cubeSize * 0.5f, 0f), cube, cubeSize);
    }
}
