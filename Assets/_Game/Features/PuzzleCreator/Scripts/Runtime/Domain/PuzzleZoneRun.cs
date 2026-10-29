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

        internal static bool Simulate(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleZone[] zones,
            out PuzzleSolution solution, out TrajectoryResult result)
        {
            SpaceShapingSession session = new(SpaceShapingSetup.Build(puzzle, settings));
            for (int i = 0; i < zones.Length; i++)
                session.ApplyZone(zones[i].ToSpaceZone());

            result = session.Simulate();
            solution = new PuzzleSolution
            {
                Zones = zones,
                ShipTime = result.ShipTime,
                EntropySpent = session.EntropySpent
            };

            return result.Outcome == TrajectoryOutcome.Arrive;
        }
    }
}
