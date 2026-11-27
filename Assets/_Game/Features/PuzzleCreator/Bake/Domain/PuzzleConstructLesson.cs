using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Applies the lesson's moves to the generated field. A move only repositions an existing planet,
    // so the contract's PlanetCount is never changed; endpoints and the shared hub are out of bounds
    // for a move.
    public static class PuzzleConstructLesson
    {
        public static PuzzleLessonApplied Apply(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request,
            PuzzleConstructedShip[] ships, int hubIndex, Vector3 hubHome)
        {
            PuzzleLessonContext context = Context(planets, request, ships, hubIndex, hubHome);
            PuzzleLessonPlan plan = request.Lesson != null
                ? request.Lesson.Place(context)
                : PuzzleLessonPlan.None;
            if (plan == null)
                return null;

            bool[] isProtected = Protected(planets, ships);
            if (!ApplyMoves(planets, plan.Moves, isProtected))
                return null;

            // The hub is protected after the moves: the lesson may move it onto its answer, but the
            // decoy step must never touch it.
            if (hubIndex >= 0)
                isProtected[hubIndex] = true;

            return new PuzzleLessonApplied { Plan = plan, Protected = isProtected };
        }

        private static bool[] Protected(
            PuzzleFieldPlanet[] planets, PuzzleConstructedShip[] ships)
        {
            bool[] isProtected = new bool[planets.Length];
            for (int i = 0; i < ships.Length; i++)
            {
                isProtected[ships[i].OriginIndex] = true;
                isProtected[ships[i].DestinationIndex] = true;
            }

            return isProtected;
        }

        private static PuzzleLessonContext Context(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request,
            PuzzleConstructedShip[] ships, int hubIndex, Vector3 hubHome)
        {
            PuzzleFieldBake.CubeRange(planets, out float min, out float max);
            PuzzleConstructedShip first = ships[0];
            return new PuzzleLessonContext
            {
                Planets = planets,
                OriginIndex = first.OriginIndex,
                DestinationIndex = first.DestinationIndex,
                Origin = first.Origin,
                Destination = first.Destination,
                Ships = ships,
                HubIndex = hubIndex,
                HubPosition = hubIndex >= 0 ? hubHome : Vector3.zero,
                CaptureRadius = first.CaptureRadius,
                CubeSizeMin = min,
                CubeSizeMax = max,
                SunRadius = request.Brief.SunRadius,
                Brief = request.Brief,
                Settings = request.Sim,
                SeedValue = PuzzleConstructSeeds.Value(request.Seed.Seed, request.Attempt)
            };
        }

        private static bool ApplyMoves(
            PuzzleFieldPlanet[] planets, PuzzleLessonMove[] moves, bool[] isProtected)
        {
            if (moves == null)
                return true;

            for (int i = 0; i < moves.Length; i++)
            {
                PuzzleLessonMove move = moves[i];
                if (move.Index < 0 || move.Index >= planets.Length || isProtected[move.Index])
                    return false;

                planets[move.Index] = new PuzzleFieldPlanet(
                    move.Home, move.Radius, planets[move.Index].name);
                isProtected[move.Index] = true;
            }

            return true;
        }
    }
}
