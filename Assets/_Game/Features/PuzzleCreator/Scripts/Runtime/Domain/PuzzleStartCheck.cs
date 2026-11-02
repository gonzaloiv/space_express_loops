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

            if (!brief.NoFail)
                return PuzzleBakeStatus.Baked;

            return start.StartOutcome == TrajectoryOutcome.Crash
                ? PuzzleBakeStatus.Baked
                : PuzzleBakeStatus.StartNotBlocked;
        }
    }
}
