using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // A fan-out's lesson: the shared origin and the bend near it. The start must fail for every ship,
    // the grow lands all of them, and every path passes the bend on one side.
    public sealed class PuzzleFanOutCurveLesson : IPuzzleLesson
    {
        public PuzzleLessonPlan Place(PuzzleLessonContext context)
        {
            if (!Matches(context))
                return null;

            PuzzleFieldPlanet[] planets = (PuzzleFieldPlanet[])context.Planets.Clone();
            if (!PuzzleLessonPlacement.TryAt(context, planets, context.HubIndex, context.HubPosition))
                return null;

            SpaceShapingSetup setup = PuzzleLessonRun.Setup(planets, context);
            if (!Starts(setup, context))
                return null;
            if (!TryAnswer(planets, context, setup, out PuzzleZone answer))
                return null;
            if (!Holds(setup, planets, context, answer))
                return null;

            PuzzleFieldPlanet hub = planets[context.HubIndex];
            return new PuzzleLessonPlan(
                new[] { new PuzzleLessonMove(context.HubIndex, hub.position, hub.radius) },
                new[] { answer }, PuzzleSolutionKind.Curve);
        }

        // Every answered path must pass the bend on the same side, or one grow cannot serve the fan.
        public static bool PassesOnOneSide(TrajectoryResult[] results, Vector3 bend)
        {
            int side = 0;
            for (int i = 0; i < results.Length; i++)
            {
                int current = PuzzleRunGeometry.SideOf(results[i], bend);
                if (current == 0)
                    return false;
                if (side == 0)
                    side = current;
                else if (current != side)
                    return false;
            }

            return side != 0;
        }

        private static bool Matches(PuzzleLessonContext context) =>
            context.Brief.Relation == PuzzleShipRelation.FanOut
            && context.HubIndex >= 0
            && context.Ships != null
            && context.Ships.Length >= 2;

        private static bool Starts(SpaceShapingSetup setup, PuzzleLessonContext context)
        {
            TrajectoryResult[] starts = PuzzleLessonRun.StartResults(setup);
            return PuzzleLessonRun.StartsAsTheBriefAsks(starts, context)
                && PuzzleLessonRun.StartsFailForEveryShip(starts);
        }

        private static bool TryAnswer(
            PuzzleFieldPlanet[] planets, PuzzleLessonContext context, SpaceShapingSetup setup,
            out PuzzleZone answer)
        {
            float radius = planets[context.HubIndex].radius;
            PuzzleZone zone = PuzzleZoneCandidates.Zone(
                planets[context.HubIndex].position, PuzzleLessonAnswer.Size(context, radius), 1f,
                context.Settings.ZoneFalloff);
            answer = default;
            if (PuzzleLessonAnswer.Rejected(context, zone))
                return false;
            if (PuzzleLessonAnswer.HoldsAnother(context.HubIndex, planets, zone))
                return false;

            return PuzzleLessonScale.TryMiddle(
                setup, zone, PuzzleLessonAnswer.ScaleFloor(context),
                context.Settings.MaxScaleFactor, out answer);
        }

        private static bool Holds(
            SpaceShapingSetup setup, PuzzleFieldPlanet[] planets, PuzzleLessonContext context,
            PuzzleZone answer)
        {
            TrajectoryResult[] answered = PuzzleLessonRun.AnswerResults(setup, answer);
            if (!answered.AllArrived())
                return false;

            return PassesOnOneSide(answered, planets[context.HubIndex].position);
        }
    }
}
