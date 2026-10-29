using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleWindow
    {
        public static bool TryCrossing(
            PuzzleData field, PuzzleSimSettings settings, out float shipTime)
        {
            shipTime = 0f;
            if (settings.NominalSpeed <= 0f)
                return false;

            SpaceShapingSetup setup = SpaceShapingSetup.Build(field, settings);
            setup.LaunchSpeed = settings.NominalSpeed;
            SpaceShapingSession session = new(setup);
            TrajectoryResult result = session.Simulate();
            shipTime = result.ShipTime;
            return result.Outcome == TrajectoryOutcome.Arrive;
        }

        public static void SetCrossing(PuzzleData field, float crossing, float tightness)
        {
            Set(field, crossing, HalfWidth(crossing, tightness));
        }

        public static bool SetSolution(
            PuzzleData field, float crossing, float tightness, float shipTime)
        {
            float halfWidth = HalfWidth(crossing, tightness);
            if (halfWidth <= 0f)
                return false;

            Set(field, shipTime, halfWidth);
            return true;
        }

        public static float HalfWidth(float crossing, float tightness) =>
            crossing <= 0f ? 0f : Mathf.Clamp01(tightness) * crossing;

        internal static bool Contains(PuzzleData field, float shipTime) =>
            shipTime >= field.tmin && shipTime <= field.tmax;

        private static void Set(PuzzleData field, float centre, float halfWidth)
        {
            field.tmin = centre - halfWidth;
            field.tmax = centre + halfWidth;
        }
    }
}
