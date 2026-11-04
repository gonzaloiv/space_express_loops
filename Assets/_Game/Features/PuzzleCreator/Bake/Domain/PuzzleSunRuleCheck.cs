using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSunRuleCheck
    {
        public static bool Matches(PuzzleData puzzle, PuzzleZone[] solution, PuzzleSunRule rule)
        {
            if (rule == PuzzleSunRule.Any)
                return true;

            if (puzzle == null || solution == null)
                return false;

            int sun = SunIndex(puzzle.sources);
            if (sun < 0)
                return false;

            PuzzleSource sunSource = puzzle.sources[sun];
            for (int i = 0; i < solution.Length; i++)
            {
                if (!solution[i].Contains(sunSource.home))
                    continue;

                if (rule == PuzzleSunRule.InZone)
                    return true;

                if (rule == PuzzleSunRule.OutOfZone)
                    return false;

                if ((solution[i].centre - sunSource.home).sqrMagnitude < 0.0001f
                    && solution[i].scale > 1f)
                    return true;
            }

            return rule == PuzzleSunRule.OutOfZone;
        }

        private static int SunIndex(PuzzleSource[] sources)
        {
            if (sources == null)
                return -1;

            for (int i = 0; i < sources.Length; i++)
                if (sources[i].centreOnly)
                    return i;

            return -1;
        }
    }
}
