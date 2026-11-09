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

        // The index-space fraction (0-1 over the point list) where a ship parks `gap` metres before
        // the end of the drawn run. Used only on a landing so the ship stops clear of the planet.
        public static float ParkProgress(IReadOnlyList<Vector3> points, float gap)
        {
            int last = points.Count - 1;
            if (last < 1 || gap <= 0f)
                return 1f;

            float total = 0f;
            for (int i = 0; i < last; i++)
                total += Vector3.Distance(points[i], points[i + 1]);

            float target = Mathf.Max(total - gap, 0f);
            float walked = 0f;
            for (int i = 0; i < last; i++)
            {
                float segment = Vector3.Distance(points[i], points[i + 1]);
                if (walked + segment >= target)
                {
                    float within = segment > 0.000001f ? (target - walked) / segment : 0f;
                    return (i + within) / last;
                }

                walked += segment;
            }

            return 1f;
        }
    }
}
