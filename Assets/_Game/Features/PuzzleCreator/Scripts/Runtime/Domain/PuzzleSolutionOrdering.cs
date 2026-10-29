using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    internal static class PuzzleSolutionOrdering
    {
        private const float CostEpsilon = 0.0001f;

        public static int Order(PuzzleSolution a, PuzzleSolution b, float timeCentre)
        {
            int cost = a.EntropySpent.CompareTo(b.EntropySpent);
            if (cost != 0)
                return cost;

            float aDistance = Mathf.Abs(a.ShipTime - timeCentre);
            float bDistance = Mathf.Abs(b.ShipTime - timeCentre);
            return aDistance.CompareTo(bDistance);
        }

        public static bool Precedes(PuzzleSolution a, PuzzleSolution b, float timeCentre)
        {
            float delta = a.EntropySpent - b.EntropySpent;
            if (delta < -CostEpsilon)
                return true;

            if (delta > CostEpsilon)
                return false;

            return Mathf.Abs(a.ShipTime - timeCentre) < Mathf.Abs(b.ShipTime - timeCentre);
        }

        public static float WindowCentre(PuzzleData puzzle) => (puzzle.tmin + puzzle.tmax) * 0.5f;
    }
}
