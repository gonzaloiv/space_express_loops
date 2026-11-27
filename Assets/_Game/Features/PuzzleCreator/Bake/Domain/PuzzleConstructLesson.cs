using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Applies the lesson's moves to the generated field. A move only repositions an existing planet,
    // so the contract's PlanetCount is never changed; endpoints are out of bounds for a move.
    public static class PuzzleConstructLesson
    {
        public static PuzzleLessonApplied Apply(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request,
            int originIndex, int destinationIndex)
        {
            PuzzleLessonPlan plan = request.Lesson?.Place(
                Context(planets, request, originIndex, destinationIndex)) ?? PuzzleLessonPlan.None;
            bool[] isProtected = new bool[planets.Length];
            isProtected[originIndex] = true;
            isProtected[destinationIndex] = true;
            if (!ApplyMoves(planets, plan.Moves, isProtected))
                return null;

            return new PuzzleLessonApplied { Plan = plan, Protected = isProtected };
        }

        private static PuzzleLessonContext Context(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request,
            int originIndex, int destinationIndex)
        {
            PuzzleFieldBake.CubeRange(planets, out float min, out float max);
            return new PuzzleLessonContext
            {
                Planets = planets,
                OriginIndex = originIndex,
                DestinationIndex = destinationIndex,
                Origin = planets[originIndex].position,
                Destination = planets[destinationIndex].position,
                CubeSizeMin = min,
                CubeSizeMax = max,
                SunRadius = request.Brief.SunRadius,
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
