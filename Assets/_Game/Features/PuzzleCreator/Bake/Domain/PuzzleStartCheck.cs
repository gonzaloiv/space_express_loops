using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleStartCheck
    {
        private const float MinStartAngle = 4f;
        private const float MinStartDeviationFraction = 0.05f;

        public static PuzzleBakeStatus Status(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief)
        {
            PuzzleVerification start = PuzzleVerifier.Verify(field, null, settings);
            if (!start.StartFails)
                return PuzzleBakeStatus.StartNotFailed;

            if (brief.NoFail && start.StartOutcome != TrajectoryOutcome.Crash)
                return PuzzleBakeStatus.StartNotBlocked;

            if (!NeedsCurvedStart(brief.Mechanism))
                return PuzzleBakeStatus.Baked;

            float run = Vector3.Distance(field.origin, field.destination);
            float deviation = PuzzleRunGeometry.MaxChordDeviation(
                start.Start, field.origin, field.destination);
            if (run > 0f && deviation / run < MinStartDeviationFraction)
                return PuzzleBakeStatus.StartTooStraight;

            if (PuzzleRunGeometry.MaxAngle(start.Start, field.origin, field.destination)
                < MinStartAngle)
                return PuzzleBakeStatus.StartTooStraight;

            return PuzzleBakeStatus.Baked;
        }

        private static bool NeedsCurvedStart(PuzzleMechanism mechanism) =>
            mechanism == PuzzleMechanism.Curve || mechanism == PuzzleMechanism.Mixed;
    }
}
