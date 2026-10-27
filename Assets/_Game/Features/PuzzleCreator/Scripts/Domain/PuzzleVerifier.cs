using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleVerifier
    {
        public static PuzzleVerification Verify(PuzzleData puzzle, PuzzleSimSettings settings)
        {
            SpaceShapingSetup setup = SpaceShapingSetup.Build(puzzle, settings);

            SpaceShapingSession start = new(setup);
            TrajectoryResult startResult = Simulate(start);

            SpaceShapingSession solved = new(setup);
            Apply(solved, puzzle.solution);
            TrajectoryResult solvedResult = Simulate(solved);

            return new PuzzleVerification
            {
                Start = startResult,
                SolvedRun = solvedResult,
                EntropySpent = solved.EntropySpent,
                StartMargin = ClosestApproach(startResult, setup.Destination),
                Solved = solved.IsSolved(solvedResult) && solved.EntropySpent <= setup.EntropyBudget
            };
        }

        private static TrajectoryResult Simulate(SpaceShapingSession session) =>
            session.Simulate(session.BuildField(), new TrajectoryIntegrator(), new ShipLaunch());

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
