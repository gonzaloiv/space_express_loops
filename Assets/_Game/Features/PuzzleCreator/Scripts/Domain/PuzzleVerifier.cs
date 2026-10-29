using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleVerifier
    {
        private const float Tolerance = 0.0001f;

        public static PuzzleVerification Verify(
            PuzzleData puzzle, PuzzleZone[] solution, PuzzleSimSettings settings)
        {
            SpaceShapingSetup setup = SpaceShapingSetup.Build(puzzle, settings);
            SpaceShapingSession start = new(setup);
            TrajectoryResult startResult = start.Simulate();
            return VerifyRun(puzzle, settings, setup, solution, startResult);
        }

        private static PuzzleVerification VerifyRun(
            PuzzleData puzzle, PuzzleSimSettings settings, SpaceShapingSetup setup,
            PuzzleZone[] solution, TrajectoryResult startResult)
        {
            SpaceShapingSession solved = new(setup);
            Apply(solved, solution);
            TrajectoryResult solvedResult = solved.Simulate();
            return new PuzzleVerification
            {
                Start = startResult,
                SolvedRun = solvedResult,
                Solution = solution,
                EntropySpent = solved.EntropySpent,
                StartMargin = startResult.HasPoints
                    ? startResult.ClosestApproach(setup.Destination)
                    : 0f,
                StartFails = StartFailed(startResult, puzzle),
                Solved = solved.IsSolved(solvedResult)
                    && solved.EntropySpent <= setup.EntropyBudget
                    && Fits(solution, puzzle, settings)
            };
        }

        private static bool Fits(
            PuzzleZone[] solution, PuzzleData puzzle, PuzzleSimSettings settings)
        {
            if (solution == null || solution.Length != puzzle.moves)
                return false;

            for (int i = 0; i < solution.Length; i++)
            {
                float size = solution[i].halfExtents.x * 2f;
                if (size < puzzle.cubeSizeMin - Tolerance || size > puzzle.cubeSizeMax + Tolerance)
                    return false;

                if (solution[i].scale < settings.MinScaleFactor - Tolerance
                    || solution[i].scale > settings.MaxScaleFactor + Tolerance)
                    return false;

                if (!puzzle.resizable && !SameSize(solution[0], solution[i]))
                    return false;

                if (PuzzleZoneCandidates.ContainsEndpoint(solution[i], puzzle))
                    return false;
            }

            return true;
        }

        private static bool SameSize(PuzzleZone a, PuzzleZone b) =>
            Mathf.Abs(a.halfExtents.x - b.halfExtents.x) <= Tolerance;

        private static bool StartFailed(TrajectoryResult result, PuzzleData puzzle) =>
            result.Outcome != TrajectoryOutcome.Crash
            && !(result.Outcome == TrajectoryOutcome.Arrive
                && PuzzleWindow.Contains(puzzle, result.ShipTime));

        private static void Apply(SpaceShapingSession session, PuzzleZone[] zones)
        {
            if (zones == null)
                return;

            for (int i = 0; i < zones.Length; i++)
                session.ApplyZone(zones[i].ToSpaceZone());
        }
    }
}
