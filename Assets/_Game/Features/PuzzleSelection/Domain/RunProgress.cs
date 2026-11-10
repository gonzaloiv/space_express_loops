using System;

namespace DigitalLove.Game.PuzzleSelection
{
    public readonly struct RunProgress
    {
        private readonly bool[] solved;
        private readonly int[] stars;

        public int CurrentIndex { get; }
        public int Count { get; }

        private RunProgress(int currentIndex, int count, bool[] solved, int[] stars)
        {
            CurrentIndex = Clamp(currentIndex, count);
            Count = Math.Max(count, 0);
            this.solved = CopySolved(solved, Count);
            this.stars = CopyStars(stars, Count);
        }

        public bool IsSolved(int index) => IsInRange(index) && solved[index];

        public int Stars(int index) => IsInRange(index) ? stars[index] : 0;

        public bool IsFresh
        {
            get
            {
                for (int i = 0; i < solved.Length; i++)
                {
                    if (solved[i])
                        return false;
                }

                return CurrentIndex <= 0;
            }
        }

        public static RunProgress FromSolved(
            int count, int currentIndex, bool[] solvedFlags, int[] starValues = null) =>
            new(currentIndex, count, solvedFlags, starValues);

        private bool IsInRange(int index) => index >= 0 && index < solved.Length;

        private static int Clamp(int index, int count)
        {
            if (count <= 0)
                return 0;

            return Math.Min(Math.Max(index, 0), count - 1);
        }

        private static bool[] CopySolved(bool[] source, int count)
        {
            bool[] result = new bool[count];
            if (source == null)
                return result;

            int length = Math.Min(source.Length, count);
            for (int i = 0; i < length; i++)
                result[i] = source[i];

            return result;
        }

        private static int[] CopyStars(int[] source, int count)
        {
            int[] result = new int[count];
            if (source == null)
                return result;

            int length = Math.Min(source.Length, count);
            for (int i = 0; i < length; i++)
                result[i] = source[i];

            return result;
        }
    }
}
