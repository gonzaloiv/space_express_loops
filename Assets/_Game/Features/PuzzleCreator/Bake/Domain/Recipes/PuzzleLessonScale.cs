using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleLessonScale
    {
        public const int Samples = 64;
        private const int MinRun = 3;

        public static bool TryMiddle(
            SpaceShapingSetup setup, PuzzleZone zone, float min, float max,
            out PuzzleZone answer)
        {
            answer = zone;
            if (max <= min)
                return false;

            bool[] arrives = Arrivals(setup, zone, min, max);
            if (!Longest(arrives, out int start, out int length) || length < MinRun)
                return false;

            int middle = start + (length - 1) / 2;
            answer = Scaled(zone, Mathf.Lerp(min, max, middle / (float)(Samples - 1)));
            return true;
        }

        public static bool AnyArrives(
            SpaceShapingSetup setup, PuzzleZone zone, float min, float max)
        {
            if (max <= min)
                return false;

            bool[] arrives = Arrivals(setup, zone, min, max);
            for (int i = 0; i < arrives.Length; i++)
            {
                if (arrives[i])
                    return true;
            }

            return false;
        }

        private static bool[] Arrivals(
            SpaceShapingSetup setup, PuzzleZone zone, float min, float max)
        {
            bool[] arrives = new bool[Samples];
            for (int i = 0; i < Samples; i++)
            {
                float scale = Mathf.Lerp(min, max, i / (float)(Samples - 1));
                arrives[i] = PuzzleLessonRun.Arrives(setup, Scaled(zone, scale));
            }

            return arrives;
        }

        private static PuzzleZone Scaled(PuzzleZone zone, float scale)
        {
            zone.scale = scale;
            return zone;
        }

        private static bool Longest(bool[] arrives, out int start, out int length)
        {
            start = -1;
            length = 0;
            for (int i = 0; i < arrives.Length; i++)
            {
                if (!arrives[i])
                    continue;

                int begin = i;
                while (i + 1 < arrives.Length && arrives[i + 1])
                    i++;
                int current = i - begin + 1;
                if (current <= length)
                    continue;

                start = begin;
                length = current;
            }

            return start >= 0;
        }
    }
}
