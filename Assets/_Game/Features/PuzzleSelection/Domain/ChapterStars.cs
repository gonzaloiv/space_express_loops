using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // The chapter's star total: three per graded job, filled by the best solve so far.
    public readonly struct ChapterStars
    {
        public ChapterStars(int earned, int possible, int gradedJobs)
        {
            Possible = Mathf.Max(possible, 0);
            GradedJobs = Mathf.Max(gradedJobs, 0);
            Earned = Mathf.Clamp(earned, 0, Possible);
        }

        public int Earned { get; }

        public int Possible { get; }

        public int GradedJobs { get; }

        // The marks between the jobs on the meter: one fewer than the graded jobs.
        public int JobBoundaries => Mathf.Max(GradedJobs - 1, 0);

        public float Share => Possible > 0 ? Earned / (float)Possible : 0f;

        public static ChapterStars From(PuzzleMap map)
        {
            int earned = 0;
            int possible = 0;
            int graded = 0;
            for (int i = 0; i < map.Count; i++)
            {
                JobEntry entry = map[i];
                if (entry.NoGrade)
                    continue;

                graded++;
                possible += 3;
                earned += Mathf.Clamp(entry.Stars, 0, 3);
            }

            return new ChapterStars(earned, possible, graded);
        }
    }
}
