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
            TrajectoryResult startResult = Simulate(start);

            SpaceShapingSession solved = new(setup);
            Apply(solved, solution);
            TrajectoryResult solvedResult = Simulate(solved);

            return new PuzzleVerification
            {
                Start = startResult,
                SolvedRun = solvedResult,
                Solution = solution,
                EntropySpent = solved.EntropySpent,
                StartMargin = ClosestApproach(startResult, setup.Destination),
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

            if (!MatchesFixed(solution, puzzle))
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
            }

            return true;
        }

        private static bool SameSize(PuzzleZone a, PuzzleZone b) =>
            Mathf.Abs(a.halfExtents.x - b.halfExtents.x) <= Tolerance;

        private static bool MatchesFixed(PuzzleZone[] solution, PuzzleData puzzle)
        {
            if (puzzle.fixedZones == null || puzzle.fixedZones.Length == 0)
                return true;

            if (solution.Length != puzzle.fixedZones.Length)
                return false;

            for (int i = 0; i < solution.Length; i++)
            {
                if ((solution[i].centre - puzzle.fixedZones[i].centre).sqrMagnitude
                    > Tolerance * Tolerance)
                    return false;

                if ((solution[i].halfExtents - puzzle.fixedZones[i].halfExtents).sqrMagnitude
                    > Tolerance * Tolerance)
                    return false;
            }

            return true;
        }

        private static TrajectoryResult Simulate(SpaceShapingSession session) =>
            session.Simulate(session.BuildField(), new TrajectoryIntegrator(), new ShipLaunch());

        private static bool StartFailed(TrajectoryResult result, PuzzleData puzzle) =>
            result.Outcome != TrajectoryOutcome.Crash
            && !(result.Outcome == TrajectoryOutcome.Arrive
                && result.ShipTime >= puzzle.tmin
                && result.ShipTime <= puzzle.tmax);

        private static void Apply(SpaceShapingSession session, PuzzleZone[] zones)
        {
            if (zones == null)
                return;

            for (int i = 0; i < zones.Length; i++)
                session.ApplyZone(zones[i].ToSpaceZone());
        }

        private static float ClosestApproach(TrajectoryResult result, Vector3 destination)
        {
            if (result.Points == null || result.Points.Count == 0)
                return 0f;

            float closest = float.MaxValue;
            for (int i = 0; i < result.Points.Count; i++)
                closest = Mathf.Min(closest, Vector3.Distance(result.Points[i], destination));

            return closest;
        }
    }
}
