using UnityEngine;

namespace DigitalLove.Game.GesturePrompts.Editor
{
    // Everything the rig can see in one frame. Null masks are simply absent.
    public readonly struct PromptMasks
    {
        public PromptMasks(
            bool[] hands, bool[] cube, bool[] meter = null, bool[] star = null, float starAlpha = 1f)
        {
            Hands = hands;
            Cube = cube;
            Meter = meter;
            Star = star;
            StarAlpha = starAlpha;
        }

        public bool[] Hands { get; }

        public bool[] Cube { get; }

        public bool[] Meter { get; }

        public bool[] Star { get; }

        public float StarAlpha { get; }
    }

    // Turns the hand, cube and meter masks into the prompt's line art: the module draws the hands and
    // the outline; this wrapper supplies the game's cube, meter and star as the background alpha.
    public static class LineArt
    {
        private const float CubeAlpha = 0.5f;
        private const float MeterAlpha = 0.8f;

        public static Color32[] Compose(PromptMasks masks, int size, int line, int downsample)
        {
            float[] overlay = new float[masks.Hands.Length];
            for (int i = 0; i < overlay.Length; i++)
            {
                float value = 0f;
                if (masks.Cube != null && masks.Cube[i])
                    value = CubeAlpha;
                if (masks.Star != null && masks.Star[i])
                    value = Mathf.Max(value, masks.StarAlpha);
                if (masks.Meter != null && masks.Meter[i])
                    value = Mathf.Max(value, MeterAlpha);
                overlay[i] = value;
            }

            return DigitalLove.XR.GestureArt.Editor.LineArt.Compose(
                masks.Hands, overlay, size, line, downsample);
        }
    }
}
