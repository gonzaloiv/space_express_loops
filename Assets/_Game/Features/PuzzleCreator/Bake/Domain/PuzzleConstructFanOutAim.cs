using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The fan-out's aim: build the bend's provisional answer, see where each answered path runs, and
    // move every destination to the nearest free spot of the same generated shape. A move changes the
    // launch, so it re-simulates per ship; it stops on a landing plateau, not a single landing.
    public static class PuzzleConstructFanOutAim
    {
        public const int Steps = 4;

        public static bool TryAim(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, int seedValue,
            int origin, int bend, PuzzleConstructedShip[] ships)
        {
            for (int step = 0; step < Steps; step++)
            {
                if (Converged(planets, request, ships, bend))
                    return true;
                if (!Move(planets, request, seedValue, step, origin, bend, ships))
                    return false;
            }

            return Converged(planets, request, ships, bend);
        }

        public static int ShapeSeed(int seedValue, int step) =>
            PuzzleConstructSeeds.Value(seedValue, step + 1);

        private static bool Converged(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request,
            PuzzleConstructedShip[] ships, int bend)
        {
            if (!TryAnswer(planets, request, ships, bend,
                    out SpaceShapingSetup setup, out PuzzleZone answer))
                return false;
            if (!PuzzleLessonRun.StartsFailForEveryShip(PuzzleLessonRun.StartResults(setup)))
                return false;
            if (!PuzzleLessonRun.Arrives(setup, answer))
                return false;

            return PuzzleFanOutCurveLesson.PassesOnOneSide(
                PuzzleLessonRun.AnswerResults(setup, answer), planets[bend].position);
        }

        private static bool Move(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, int seedValue, int step,
            int origin, int bend, PuzzleConstructedShip[] ships)
        {
            PuzzleFieldPlanet[] shape =
                PuzzleConstructShape.Generate(request, ShapeSeed(seedValue, step));
            for (int i = 0; i < ships.Length; i++)
            {
                if (!TryZone(planets, request, ships, bend,
                        out SpaceShapingSetup setup, out PuzzleZone zone))
                    return false;

                TrajectoryResult[] results = PuzzleLessonRun.AnswerResults(setup, zone);
                Vector3 target = PuzzleRunGeometry.ClosestPoint(results[i], ships[i].Destination);
                if (!TrySpot(planets, request, shape, origin, ships, i, target, out Vector3 home))
                    return false;

                int destination = ships[i].DestinationIndex;
                ships[i].Destination = home;
                planets[destination] = new PuzzleFieldPlanet(
                    home, planets[destination].radius, planets[destination].name);
            }

            return true;
        }

        private static bool TryAnswer(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request,
            PuzzleConstructedShip[] ships, int bend,
            out SpaceShapingSetup setup, out PuzzleZone answer)
        {
            if (!TryZone(planets, request, ships, bend, out setup, out PuzzleZone zone))
            {
                answer = default;
                return false;
            }

            PuzzleLessonContext context = PuzzleConstructLesson.Context(
                planets, request, ships, bend, planets[bend].position);
            return PuzzleLessonScale.TryMiddle(
                setup, zone, PuzzleLessonAnswer.ScaleFloor(context),
                context.Settings.MaxScaleFactor, out answer);
        }

        // The provisional zone: a cube at the middle of the usable range, so an answered path exists
        // before any landing plateau does. The lesson later re-checks the plateau.
        private static bool TryZone(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request,
            PuzzleConstructedShip[] ships, int bend,
            out SpaceShapingSetup setup, out PuzzleZone zone)
        {
            PuzzleLessonContext context = PuzzleConstructLesson.Context(
                planets, request, ships, bend, planets[bend].position);
            setup = PuzzleLessonRun.Setup(planets, context);
            float radius = planets[bend].radius;
            zone = PuzzleZoneCandidates.Zone(
                planets[bend].position, PuzzleLessonAnswer.Size(context, radius), 1f,
                context.Settings.ZoneFalloff);
            if (PuzzleLessonAnswer.Rejected(context, zone))
                return false;
            if (PuzzleLessonAnswer.HoldsAnother(bend, planets, zone))
                return false;

            zone.scale = Mathf.Lerp(
                PuzzleLessonAnswer.ScaleFloor(context), context.Settings.MaxScaleFactor, 0.5f);
            return true;
        }

        private static bool TrySpot(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, PuzzleFieldPlanet[] shape,
            int origin, PuzzleConstructedShip[] ships, int ship, Vector3 target, out Vector3 home)
        {
            home = target;
            float best = float.MaxValue;
            bool found = false;
            for (int i = 0; i < shape.Length; i++)
            {
                Vector3 candidate = shape[i].position;
                float squared = (candidate - target).sqrMagnitude;
                if (squared >= best || !Fits(planets, request, origin, ships, ship, candidate))
                    continue;

                best = squared;
                home = candidate;
                found = true;
            }

            return found;
        }

        private static bool Fits(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, int origin,
            PuzzleConstructedShip[] ships, int ship, Vector3 candidate)
        {
            PuzzleFieldPlanet[] copy = (PuzzleFieldPlanet[])planets.Clone();
            int destination = ships[ship].DestinationIndex;
            copy[destination] = new PuzzleFieldPlanet(
                candidate, planets[destination].radius, planets[destination].name);
            if (!PuzzleConstructChord.PairFits(copy, request, origin, destination))
                return false;

            for (int i = 0; i < ships.Length; i++)
            {
                if (i == ship)
                    continue;

                Vector3 fan = candidate - ships[i].Origin;
                Vector3 other = ships[i].Destination - ships[i].Origin;
                if (Vector3.Angle(fan, other) > PuzzleConstructFanOutChord.ConeDegrees)
                    return false;
            }

            return true;
        }
    }
}
