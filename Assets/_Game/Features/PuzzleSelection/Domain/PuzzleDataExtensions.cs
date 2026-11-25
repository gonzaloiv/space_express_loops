using DigitalLove.Game.SpaceShaping;

namespace DigitalLove.Game.PuzzleSelection
{
    public static class PuzzleDataExtensions
    {
        public static bool HasCentredSun(this PuzzleData puzzle)
        {
            if (puzzle == null || puzzle.sources == null)
                return false;

            for (int i = 0; i < puzzle.sources.Length; i++)
            {
                if (!puzzle.sources[i].centreOnly)
                    continue;

                return puzzle.sources[i].home.sqrMagnitude < 0.000001f;
            }

            return false;
        }
    }
}
