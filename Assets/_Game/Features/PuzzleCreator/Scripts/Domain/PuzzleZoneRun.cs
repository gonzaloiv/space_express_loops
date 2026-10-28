namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleZoneRun
    {
        public static bool TryArrive(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleZone zone, out PuzzleSolution solution)
        {
            SpaceShapingSession session = new(SpaceShapingSetup.Build(puzzle, settings));
            session.ApplyZone(zone.ToSpaceZone());
            TrajectoryResult result = session.Simulate(
                session.BuildField(), new TrajectoryIntegrator(), new ShipLaunch());
            solution = new PuzzleSolution
            {
                Zones = new[] { zone },
                ShipTime = result.ShipTime,
                EntropySpent = session.EntropySpent
            };

            return result.Outcome == TrajectoryOutcome.Arrive;
        }
    }
}
