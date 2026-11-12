using DigitalLove.Casual.Levels;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Which palette entry a job starts at, and how its routes step through the palette.
    public static class RoutePalette
    {
        private const int FirstJob = 1;

        public static int StartIndex(string levelId)
        {
            int job;
            return LevelData.TryAsIndex(levelId, out job) ? Mathf.Max(job - FirstJob, 0) : 0;
        }

        public static int Entry(int start, int route, int length) =>
            length > 0 ? Positive(start + route, length) : 0;

        public static Color ColorAt(ColorValue[] palette, int entry)
        {
            if (palette == null || palette.Length == 0)
                return Color.white;

            ColorValue value = palette[Positive(entry, palette.Length)];
            return value != null ? value.value : Color.white;
        }

        private static int Positive(int value, int length) => ((value % length) + length) % length;
    }
}
