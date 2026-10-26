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
                StartOutcome = startResult.Outcome,
                SolvedOutcome = solvedResult.Outcome,
                ShipTime = solvedResult.ShipTime,
                EntropySpent = solved.EntropySpent,
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
    }
}
