using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The last stage of a field bake: build the shipped variant, verify it, and apply the sun and
    // route rules. It sits beside PuzzleFieldBake because only the bake's own answers go through it.
    internal static class PuzzleFieldConfirm
    {
        public static PuzzleBakeResult Run(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief, PuzzleSolution found,
            PuzzleScanTimings timings)
        {
            long started = timings != null ? timings.Start() : 0;
            field.entropyBudget = found.EntropySpent * PuzzleGrade.BudgetFactor;
            PuzzleData variant = Variant(field, found, brief, settings);
            PuzzleVerification check = PuzzleVerifier.Verify(variant, found.Zones, settings);
            PuzzleBakeResult result = Verified(variant, found, settings, brief, check);
            timings?.Add(PuzzleScanTimings.Confirm, started);
            return result;
        }

        private static PuzzleBakeResult Verified(
            PuzzleData variant, PuzzleSolution found, PuzzleSimSettings settings, PuzzleBrief brief,
            PuzzleVerification check)
        {
            PuzzleBakeStatus status = Status(variant, found, settings, brief, check);
            if (status != PuzzleBakeStatus.Baked)
            {
                Object.DestroyImmediate(variant);
                return PuzzleFieldBake.Fail(status, check.Kind);
            }

            return new PuzzleBakeResult
            {
                Puzzle = variant,
                Solution = found.Zones,
                Status = status,
                Kind = check.Kind
            };
        }

        private static PuzzleBakeStatus Status(
            PuzzleData variant, PuzzleSolution found, PuzzleSimSettings settings, PuzzleBrief brief,
            PuzzleVerification check)
        {
            if (!check.Solved)
                return PuzzleBakeStatus.Unverified;

            if (!PuzzleSunRuleCheck.Matches(variant, found.Zones, brief.SunRule))
                return PuzzleBakeStatus.SunRuleFailed;

            if (brief.SunRule == PuzzleSunRule.NoSunAnswer
                && PuzzleSunRuleCheck.SunAnswers(variant, settings))
                return PuzzleBakeStatus.SunAnswers;

            return HasRoutes(variant, settings, brief, found.Zones)
                ? PuzzleBakeStatus.Baked
                : PuzzleBakeStatus.SingleRoute;
        }

        private static bool HasRoutes(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleBrief brief, PuzzleZone[] zones)
        {
            puzzle.routes = PuzzleRouteCount.Count(puzzle, settings, zones);
            return puzzle.routes >= Mathf.Max(1, brief.MinRoutes);
        }

        private static PuzzleData Variant(
            PuzzleData field, PuzzleSolution solution, PuzzleBrief brief, PuzzleSimSettings settings)
        {
            PuzzleData puzzle = Object.Instantiate(field);
            puzzle.frames = PuzzleFieldBake.BuildFrames(field, solution, brief, settings);
            puzzle.zones = solution.Zones.Length;
            return puzzle;
        }
    }
}
