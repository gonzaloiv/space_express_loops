using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The drawn run between two planets: clear launch and arrival caps plus the visible trim of a
    // simulated path. The simulation keeps the raw result; this is what the line and ships show.
    public static class ShipRunGeometry
    {
        public static Vector3 LaunchPoint(
            SpaceShapingSession session, ShipSpec ship, float capGap, float capGapFraction)
        {
            float radius = session.RadiusAt(ship.Origin);
            return TrajectoryCaps.LaunchPoint(
                ship.Origin, ship.Destination, radius, Gap(radius, capGap, capGapFraction));
        }

        public static Vector3 ArrivalPoint(
            SpaceShapingSession session, ShipSpec ship, float capGap, float capGapFraction)
        {
            float radius = session.RadiusAt(ship.Destination);
            return TrajectoryCaps.ArrivalPoint(
                ship.Origin, ship.Destination, radius, Gap(radius, capGap, capGapFraction));
        }

        public static TrajectoryResult[] Visible(
            SpaceShapingSession session, IReadOnlyList<TrajectoryResult> results,
            float capGap, float capGapFraction)
        {
            TrajectoryResult[] visible = new TrajectoryResult[results.Count];
            for (int i = 0; i < results.Count; i++)
                visible[i] = Clip(results[i], session, session.Ship(i), capGap, capGapFraction);

            return visible;
        }

        public static TrajectoryResult Clip(
            TrajectoryResult result, SpaceShapingSession session, ShipSpec ship,
            float capGap, float capGapFraction)
        {
            float launchRadius = session.RadiusAt(ship.Origin);
            float arrivalRadius = session.RadiusAt(ship.Destination);
            return TrajectoryCaps.Clip(
                result,
                ship.Origin, launchRadius + Gap(launchRadius, capGap, capGapFraction),
                ship.Destination, arrivalRadius + Gap(arrivalRadius, capGap, capGapFraction));
        }

        private static float Gap(float radius, float capGap, float capGapFraction) =>
            Mathf.Max(capGap, radius * capGapFraction);
    }
}
