using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSolutionSelection
    {
        public static PuzzleBakeStatus Select(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief,
            out PuzzleSolution solution, out PuzzleSolutionKind kind)
        {
            solution = default;
            kind = PuzzleSolutionKind.Other;
            PuzzleBrief any = brief;
            any.Mechanism = PuzzleMechanism.Any;
            if (!PuzzleSolutionSearch.TrySelect(field, settings, any, out PuzzleSolution cheapest))
                return PuzzleBakeStatus.NoSolution;

            if (PuzzleSolutionClassifier.Matches(brief.Mechanism, cheapest.Kind))
            {
                solution = cheapest;
                kind = cheapest.Kind;
                return PuzzleBakeStatus.Baked;
            }

            kind = cheapest.Kind;
            if (PuzzleSolutionSearch.TrySelect(field, settings, brief, out PuzzleSolution role))
            {
                Debug.Log(
                    $"[Puzzle role] cheapest {cheapest.Kind} {cheapest.EntropySpent:F3}, " +
                    $"requested {brief.Mechanism} {role.EntropySpent:F3}");
                return PuzzleBakeStatus.WrongRoleRoute;
            }

            return Mismatch(brief.Mechanism);
        }

        public static bool HasShorter(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief, PuzzleSolution found)
        {
            if (brief.Zones <= 1)
                return false;

            float width = found.Zones[0].Width;
            float budget = found.EntropySpent * PuzzleGrade.BudgetFactor;
            PuzzleBrief shorter = brief;
            shorter.Mechanism = PuzzleMechanism.Any;
            for (int zones = 1; zones < brief.Zones; zones++)
            {
                shorter.Zones = zones;
                if (PuzzleSolutionSearch.TryFindAtWidth(
                        field, settings, shorter, width, out PuzzleSolution candidate)
                    && candidate.EntropySpent <= budget)
                    return true;
            }

            return false;
        }

        private static PuzzleBakeStatus Mismatch(PuzzleMechanism mechanism) =>
            mechanism == PuzzleMechanism.Clear
                ? PuzzleBakeStatus.NotCleared
                : mechanism == PuzzleMechanism.Mixed
                    ? PuzzleBakeStatus.NotMixed
                    : PuzzleBakeStatus.NotCurved;
    }
}
