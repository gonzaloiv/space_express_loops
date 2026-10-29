namespace DigitalLove.Game.SpaceShaping.Creator
{
    internal static class PuzzleSolutionOrdering
    {
        private const float CostEpsilon = 0.0001f;

        public static int Order(PuzzleSolution a, PuzzleSolution b) =>
            a.EntropySpent.CompareTo(b.EntropySpent);

        public static bool Precedes(PuzzleSolution a, PuzzleSolution b) =>
            a.EntropySpent < b.EntropySpent - CostEpsilon;
    }
}
