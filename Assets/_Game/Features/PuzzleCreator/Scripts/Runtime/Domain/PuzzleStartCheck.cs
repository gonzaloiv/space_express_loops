namespace DigitalLove.Game.SpaceShaping.Creator
{
    internal static class PuzzleStartCheck
    {
        public static PuzzleBakeStatus Status(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief)
        {
            PuzzleVerification start = PuzzleVerifier.Verify(field, null, settings);
            if (!start.StartFails)
                return PuzzleBakeStatus.StartNotFailed;

            if (!brief.NoFail || ShowsBlocker(start, field, settings))
                return PuzzleBakeStatus.Baked;

            return PuzzleBakeStatus.StartNotBlocked;
        }

        public static bool ShowsBlocker(
            PuzzleVerification start, PuzzleData field, PuzzleSimSettings settings)
        {
            if (start.StartOutcome == TrajectoryOutcome.Crash)
                return true;

            SpaceShapingSetup setup = SpaceShapingSetup.Build(field, settings);
            bool[] obstacles = PuzzleSolutionClassifier.Obstacles(setup, start.StartResults);
            for (int i = 0; i < obstacles.Length; i++)
            {
                if (obstacles[i])
                    return true;
            }

            return false;
        }
    }
}
