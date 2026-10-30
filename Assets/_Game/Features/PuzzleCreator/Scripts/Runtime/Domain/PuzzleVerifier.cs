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
            return VerifyRun(puzzle, settings, setup, solution, start.SimulateAll());
        }

        private static PuzzleVerification VerifyRun(
            PuzzleData puzzle, PuzzleSimSettings settings, SpaceShapingSetup setup,
            PuzzleZone[] solution, TrajectoryResult[] startResults)
        {
            SpaceShapingSession solved = new(setup);
            Apply(solved, solution);
            TrajectoryResult[] solvedResults = solved.SimulateAll();
            TrajectoryResult startResult = startResults[0];
            return new PuzzleVerification
            {
                Start = startResult,
                SolvedRun = solvedResults[0],
                Solution = solution,
                EntropySpent = solved.EntropySpent,
                StartMargin = startResult.HasPoints
                    ? startResult.ClosestApproach(setup.Destination)
                    : 0f,
                StartFails = !startResults.AllArrived(),
                Solved = solvedResults.AllArrived()
                    && solved.EntropySpent <= setup.EntropyBudget
                    && Fits(solution, puzzle, settings)
            };
        }

        private static bool Fits(
            PuzzleZone[] solution, PuzzleData puzzle, PuzzleSimSettings settings)
        {
            if (solution == null || solution.Length != puzzle.zones)
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

        private static void Apply(SpaceShapingSession session, PuzzleZone[] zones)
        {
            if (zones == null)
                return;

            for (int i = 0; i < zones.Length; i++)
                session.ApplyZone(zones[i].ToSpaceZone());
        }
    }
}
