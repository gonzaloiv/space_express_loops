using DigitalLove.Game.SpaceShaping;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Two or more ships share one blocker: the hub sits inside every chord's block band, and the
    // answer is one cube shrinking it. The hub planet is the blocker, so the lesson only names it.
    public sealed class PuzzleFleetClearLesson : IPuzzleLesson
    {
        public PuzzleLessonPlan Place(PuzzleLessonContext context)
        {
            if (context.HubIndex < 0 || context.Ships == null || context.Ships.Length < 2)
                return null;

            PuzzleFieldPlanet[] planets = (PuzzleFieldPlanet[])context.Planets.Clone();
            float radius = planets[context.HubIndex].radius;
            if (!PuzzleLessonPlacement.TryAt(
                    context, planets, context.HubIndex, context.HubPosition))
                return null;

            SpaceShapingSetup setup = PuzzleLessonRun.Setup(planets, context);
            TrajectoryResult[] starts = PuzzleLessonRun.StartResults(setup);
            if (!PuzzleLessonRun.StartsAsTheBriefAsks(starts, context))
                return null;
            if (!PuzzleLessonRun.StartsFailForEveryShip(starts))
                return null;

            PuzzleZone zone = PuzzleZoneCandidates.Zone(
                context.HubPosition, PuzzleLessonAnswer.Size(context, radius), 1f,
                context.Settings.ZoneFalloff);
            if (PuzzleLessonAnswer.Rejected(context, zone))
                return null;

            float min = context.Brief.GrowOnly ? 1f : context.Settings.MinScaleFactor;
            if (!PuzzleLessonScale.TryMiddle(setup, zone, min, 1f, out PuzzleZone answer))
                return null;

            return new PuzzleLessonPlan(
                new[] { new PuzzleLessonMove(context.HubIndex, context.HubPosition, radius) },
                new[] { answer }, PuzzleSolutionKind.Clear);
        }
    }
}
