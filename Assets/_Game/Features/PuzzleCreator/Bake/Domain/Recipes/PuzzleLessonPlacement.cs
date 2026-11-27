using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleLessonPlacement
    {
        private const float MinSide = 0.000001f;

        public static int Start(PuzzleLessonContext context)
        {
            int count = context.Planets.Length;
            return count > 0 ? ((context.SeedValue % count) + count) % count : 0;
        }

        public static bool TryOnChord(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, int index, float fraction)
        {
            Vector3 home = Vector3.Lerp(Origin(context), Destination(context), fraction);
            return TryPlace(context, planets, index, home);
        }

        public static bool TryBeside(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, int index,
            float fraction, float offset)
        {
            Vector3 direction = Destination(context) - Origin(context);
            if (direction.sqrMagnitude < MinSide)
                return false;
            Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;
            if (side.sqrMagnitude < MinSide)
                return false;

            float radius = planets[index].radius;
            Vector3 home = Vector3.Lerp(Origin(context), Destination(context), fraction)
                + side * (radius * offset);
            return TryPlace(context, planets, index, home);
        }

        public static bool TryAt(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, int index, Vector3 home) =>
            TryPlace(context, planets, index, home);

        private static Vector3 Origin(PuzzleLessonContext context) =>
            context.Chord != null ? context.Chord.Origin : context.Origin;

        private static Vector3 Destination(PuzzleLessonContext context) =>
            context.Chord != null ? context.Chord.Destination : context.Destination;

        private static bool TryPlace(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, int index, Vector3 home)
        {
            float radius = planets[index].radius;
            if (!ClearOfEndpoints(context, home, radius))
                return false;
            if (!ClearOfSun(context, home, radius))
                return false;
            if (!Separated(context, planets, index, home))
                return false;

            planets[index] = new PuzzleFieldPlanet(home, radius, planets[index].name);
            return true;
        }

        private static bool ClearOfEndpoints(
            PuzzleLessonContext context, Vector3 home, float radius)
        {
            float clearance = context.Brief.EndpointClearance + radius;
            float squared = clearance * clearance;
            if (context.Ships != null && context.Ships.Length > 0)
            {
                for (int i = 0; i < context.Ships.Length; i++)
                {
                    if ((home - context.Ships[i].Origin).sqrMagnitude < squared
                        || (home - context.Ships[i].Destination).sqrMagnitude < squared)
                        return false;
                }

                return true;
            }

            return (home - context.Origin).sqrMagnitude >= squared
                && (home - context.Destination).sqrMagnitude >= squared;
        }

        private static bool ClearOfSun(PuzzleLessonContext context, Vector3 home, float radius)
        {
            if (context.Brief.SunRadius <= 0f
                || context.Brief.SunPlacement != PuzzleSunPlacement.Centre)
                return true;

            float clearance = context.Brief.SunRadius + radius;
            return home.sqrMagnitude >= clearance * clearance;
        }

        private static bool Separated(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, int index, Vector3 home)
        {
            float radius = planets[index].radius;
            for (int i = 0; i < planets.Length; i++)
            {
                if (i == index)
                    continue;

                float required = context.Settings.SeparationScale
                    * (radius + planets[i].radius);
                if ((home - planets[i].position).sqrMagnitude < required * required)
                    return false;
            }

            return true;
        }
    }
}
