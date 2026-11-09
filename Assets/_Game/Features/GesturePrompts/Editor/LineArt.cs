using UnityEngine;

namespace DigitalLove.Game.GesturePrompts.Editor
{
    // Turns the hand and cube masks into the prompt's line art: a thick outline around the hands,
    // a faint fill inside them, the cube's bars dimmer behind. White on transparent, downsampled
    // once for smooth edges.
    public static class LineArt
    {
        private const float Fill = 0.12f;
        private const float CubeAlpha = 0.5f;

        public static Color32[] Compose(bool[] hands, bool[] cube, int size, int line, int downsample)
        {
            bool[] inner = Erode(hands, size, line);
            float[] alpha = new float[hands.Length];
            for (int i = 0; i < alpha.Length; i++)
            {
                if (hands[i])
                    alpha[i] = inner[i] ? Fill : 1f;
                else if (cube[i])
                    alpha[i] = CubeAlpha;
            }

            return Downsample(alpha, size, downsample);
        }

        private static Color32[] Downsample(float[] alpha, int size, int factor)
        {
            int outSize = size / factor;
            var pixels = new Color32[outSize * outSize];
            float norm = 1f / (factor * factor);
            for (int y = 0; y < outSize; y++)
            {
                for (int x = 0; x < outSize; x++)
                {
                    float sum = 0f;
                    for (int dy = 0; dy < factor; dy++)
                    {
                        for (int dx = 0; dx < factor; dx++)
                            sum += alpha[(y * factor + dy) * size + x * factor + dx];
                    }

                    pixels[y * outSize + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(sum * norm * 255f));
                }
            }

            return pixels;
        }

        // A pixel stays only if every pixel within `radius` (a square window) is set.
        private static bool[] Erode(bool[] mask, int size, int radius)
        {
            bool[] rows = Pass(mask, size, radius, true);
            return Pass(rows, size, radius, false);
        }

        private static bool[] Pass(bool[] mask, int size, int radius, bool horizontal)
        {
            var result = new bool[mask.Length];
            int window = radius * 2 + 1;
            for (int line = 0; line < size; line++)
            {
                int count = 0;
                for (int i = -radius; i < size + radius; i++)
                {
                    if (In(mask, size, line, i + radius, horizontal))
                        count++;
                    if (In(mask, size, line, i - radius - 1, horizontal))
                        count--;
                    if (i >= 0 && i < size)
                        result[Index(size, line, i, horizontal)] = count == window;
                }
            }

            return result;
        }

        private static bool In(bool[] mask, int size, int line, int i, bool horizontal) =>
            i >= 0 && i < size && mask[Index(size, line, i, horizontal)];

        private static int Index(int size, int line, int i, bool horizontal) =>
            horizontal ? line * size + i : i * size + line;
    }
}
