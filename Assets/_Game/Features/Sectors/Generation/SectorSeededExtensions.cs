using DigitalLove.Global;

namespace DigitalLove.Game.Sectors
{
    public static class SectorSeededExtensions
    {
        public static float Range(this SeededRandom random, MinMaxFloat range) =>
            random.Range(range.min, range.max);

        public static int RangeInclusive(this SeededRandom random, MinMaxInt range) =>
            random.RangeInclusive(range.min, range.max);

        public static int RangeInclusive(this SeededRandom random, int min, int max)
        {
            if (max <= min)
                return min;

            int span = max - min + 1;
            return min + (int)((uint)random.NextInt() % (uint)span);
        }
    }
}
