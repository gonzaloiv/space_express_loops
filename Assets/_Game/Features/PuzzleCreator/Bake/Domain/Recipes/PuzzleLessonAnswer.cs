using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleLessonAnswer
    {
        public static float Size(PuzzleLessonContext context, float radius) =>
            Mathf.Clamp(
                radius * 2f * PuzzleZoneCandidates.PlanetMargin,
                context.CubeSizeMin, context.CubeSizeMax);

        public static bool Rejected(PuzzleLessonContext context, PuzzleZone zone)
        {
            if (context.Ships != null && context.Ships.Length > 0)
            {
                for (int i = 0; i < context.Ships.Length; i++)
                {
                    if (zone.Contains(context.Ships[i].Origin)
                        || zone.Contains(context.Ships[i].Destination))
                        return true;
                }
            }
            else if (zone.Contains(context.Origin) || zone.Contains(context.Destination))
                return true;

            return context.Brief.SunRule == PuzzleSunRule.OutOfZone
                && zone.Contains(Vector3.zero);
        }

        // A lesson's cube should wrap only its intended source: any other planet inside the zone would
        // scale too, turning the answer Mixed.
        public static bool HoldsAnother(
            int hubIndex, PuzzleFieldPlanet[] planets, PuzzleZone zone)
        {
            for (int i = 0; i < planets.Length; i++)
            {
                if (i != hubIndex && zone.Contains(planets[i].position))
                    return true;
            }

            return false;
        }

        // A high-scramble contract's floor: the answer's scale must move at least MinScaleDelta from 1.
        public static float ScaleFloor(PuzzleLessonContext context) =>
            Mathf.Max(1f, 1f + context.MinScaleDelta);
    }
}
