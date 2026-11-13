using System;

namespace DigitalLove.Game.PuzzleSelection
{
    // The sector's news, one line per solved job: the order is the story, so the world moves as the
    // firm works. Past the last line the wire holds on it.
    public static class SectorWire
    {
        public const int LineCount = 12;
        public const string CoverageKey = "map_coverage";

        public static string Key(int solved) =>
            $"sector_wire_{Math.Min(Math.Max(solved, 0), LineCount - 1):D2}";
    }
}
