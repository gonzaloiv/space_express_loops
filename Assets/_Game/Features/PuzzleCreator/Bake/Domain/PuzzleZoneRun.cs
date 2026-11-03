namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleZoneRun
    {
        public static bool TryArrive(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleZone zone, out PuzzleSolution solution) =>
            TryArrive(puzzle, settings, new[] { zone }, out solution);

        public static bool TryArrive(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleZone[] zones, out PuzzleSolution solution) =>
            Simulate(puzzle, settings, zones, out solution, out _);

        public static bool Simulate(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleZone[] zones,
            out PuzzleSolution solution, out TrajectoryResult result)
        {
            SpaceShapingSetup setup = SpaceShapingSetup.Build(puzzle, settings);
            SpaceShapingSession session = Applied(setup, zones);
            result = session.Simulate();
            solution = new PuzzleSolution
            {
                Zones = zones,
                EntropySpent = session.EntropySpent
            };

            return result.Outcome == TrajectoryOutcome.Arrive;
        }

        public static bool Simulate(
            PuzzleSolutionContext context, PuzzleZone[] zones,
            out PuzzleSolution solution, out TrajectoryResult result)
        {
            SpaceShapingSession session = Applied(context.Setup, zones);
            result = session.Simulate();
            solution = new PuzzleSolution
            {
                Zones = zones,
                EntropySpent = session.EntropySpent,
                Kind = context.Classify(session, result)
            };

            return result.Outcome == TrajectoryOutcome.Arrive;
        }

        private static SpaceShapingSession Applied(SpaceShapingSetup setup, PuzzleZone[] zones)
        {
            SpaceShapingSession session = new(setup);
            for (int i = 0; i < zones.Length; i++)
                session.ApplyZone(zones[i].ToSpaceZone());

            return session;
        }
    }
}
