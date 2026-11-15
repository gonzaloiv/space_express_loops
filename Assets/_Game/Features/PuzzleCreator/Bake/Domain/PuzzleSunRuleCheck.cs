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

                if (KeepsSunOut(rule))
                    return false;

                if (CentresAndEnclosesGrown(solution[i], sunSource))
                    return true;
            }

            return KeepsSunOut(rule);
        }

        // NoSunAnswer: whether a cube centred on the sun, at the job's cube size and any scale the
        // player can reach, lands the ship within budget — the shortcut the rule rejects.
        public static bool SunAnswers(PuzzleData puzzle, PuzzleSimSettings settings)
        {
            int sun = SunIndex(puzzle.sources);
            if (sun < 0)
                return false;

            float size = puzzle.frames != null && puzzle.frames.Length > 0
                ? puzzle.frames[0].size
                : puzzle.cubeSizeMax;
            return PuzzleStartFrame.SolvesInPlace(puzzle, settings, puzzle.sources[sun].home, size);
        }

        private static bool KeepsSunOut(PuzzleSunRule rule) =>
            rule == PuzzleSunRule.OutOfZone || rule == PuzzleSunRule.NoSunAnswer;

        // The cube must be centred on the star and wide enough to cover its whole surface, not only
        // its centre point, so the player sees the star inside the cube they scale.
        private static bool CentresAndEnclosesGrown(PuzzleZone zone, PuzzleSource sun) =>
            (zone.centre - sun.home).sqrMagnitude < 0.0001f
            && zone.scale > 1f
            && zone.Width >= SunEnclosingSize(sun.scale);

        // Wider than the planets' margin: the star fills most of the cube, so the extra room keeps a
        // pinch inside the cube clear of the star and easy to start.
        private const float SunCubeMargin = 1.35f;

        public static float SunEnclosingSize(float sunDiameter) => sunDiameter * SunCubeMargin;

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
