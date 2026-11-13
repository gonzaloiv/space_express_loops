using System;

namespace DigitalLove.Game.PuzzleSelection
{
    public readonly struct RunProgress
    {
        private readonly bool[] solved;
        private readonly int[] stars;

        public int CurrentIndex { get; }
        public int Count { get; }
        public int HighestOpenedIndex { get; }

        private RunProgress(
            int currentIndex, int count, bool[] solved, int[] stars, int highestOpened)
        {
            CurrentIndex = Clamp(currentIndex, count);
            Count = Math.Max(count, 0);
            HighestOpenedIndex = Math.Min(highestOpened, Count - 1);
            this.solved = CopySolved(solved, Count);
            this.stars = CopyStars(stars, Count);
        }

        public bool IsSolved(int index) => IsInRange(index) && solved[index];

        public bool IsOpened(int index) => index >= 0 && index <= HighestOpenedIndex;

        public bool IsOpenedUnsolved(int index) => IsOpened(index) && !IsSolved(index);

        public int Stars(int index) => IsInRange(index) ? stars[index] : 0;

        public int SolvedCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < solved.Length; i++)
                {
                    if (solved[i])
                        count++;
                }

                return count;
            }
        }

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

        public bool IsComplete
        {
            get
            {
                if (solved.Length == 0)
                    return false;

                for (int i = 0; i < solved.Length; i++)
                {
                    if (!solved[i])
                        return false;
                }

                return true;
            }
        }

        public static RunProgress FromSolved(
            int count, int currentIndex, bool[] solvedFlags, int[] starValues = null,
            int highestOpened = -1) =>
            new(currentIndex, count, solvedFlags, starValues, highestOpened);

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
