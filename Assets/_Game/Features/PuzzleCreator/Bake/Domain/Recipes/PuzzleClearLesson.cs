using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public sealed class PuzzleClearLesson : IPuzzleLesson
    {
        private static readonly float[] Fractions = { 0.5f, 0.34f, 0.66f };
        private static readonly float[] Offsets = { 0.5f, -0.5f, 0.75f, -0.75f, 1f, -1f };

        public PuzzleLessonPlan Place(PuzzleLessonContext context)
        {
            int offset = PuzzleLessonPlacement.Start(context);
            for (int i = 0; i < context.Planets.Length; i++)
            {
                int index = (offset + i) % context.Planets.Length;
                if (IsEndpoint(context, index))
                    continue;

                PuzzleLessonPlan plan = PlanAt(context, index);
                if (plan != null)
                    return plan;
            }

            return null;
        }

        private static PuzzleLessonPlan PlanAt(PuzzleLessonContext context, int index)
        {
            for (int f = 0; f < Fractions.Length; f++)
            {
                for (int o = 0; o < Offsets.Length; o++)
                {
                    PuzzleFieldPlanet[] planets =
                        (PuzzleFieldPlanet[])context.Planets.Clone();
                    if (!PuzzleLessonPlacement.TryBeside(
                            context, planets, index, Fractions[f], Offsets[o]))
                        continue;

                    PuzzleLessonPlan plan = Plan(context, planets, index, Fractions[f]);
                    if (plan != null)
                        return plan;
                }
            }

            return null;
        }

        private static bool IsEndpoint(PuzzleLessonContext context, int index) =>
            index == context.OriginIndex || index == context.DestinationIndex;

        private static PuzzleLessonPlan Plan(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, int index, float fraction)
        {
            int second = Second(context, planets, index);
            if (second < 0 || !PlaceSecond(context, planets, index, second, fraction))
                return null;
            if (!TryAnswer(context, planets, index, out PuzzleZone answer))
                return null;

            return new PuzzleLessonPlan(
                Moves(planets, index, second), new[] { answer }, PuzzleSolutionKind.Clear);
        }

        private static bool TryAnswer(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, int index,
            out PuzzleZone answer)
        {
            answer = default;
            float radius = planets[index].radius;
            PuzzleZone zone = PuzzleZoneCandidates.Zone(
                planets[index].position, PuzzleLessonAnswer.Size(context, radius),
                1f, context.Settings.ZoneFalloff);
            if (PuzzleLessonAnswer.Rejected(context, zone))
                return false;

            SpaceShapingSetup setup = PuzzleLessonRun.Setup(planets, context);
            if (!PuzzleLessonRun.StartsAsTheBriefAsks(setup, context, out _))
                return false;

            float min = context.Brief.GrowOnly ? 1f : context.Settings.MinScaleFactor;
            return PuzzleLessonScale.TryMiddle(setup, zone, min, 1f, out answer);
        }

        private static PuzzleLessonMove[] Moves(
            PuzzleFieldPlanet[] planets, int index, int second) => new[]
        {
            new PuzzleLessonMove(index, planets[index].position, planets[index].radius),
            new PuzzleLessonMove(second, planets[second].position, planets[second].radius)
        };

        private static int Second(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, int index)
        {
            float radius = planets[index].radius;
            int second = -1;
            for (int i = 0; i < planets.Length; i++)
            {
                if (i == index || i == context.OriginIndex || i == context.DestinationIndex
                    || planets[i].radius > radius)
                    continue;
                if (second < 0 || planets[i].radius > planets[second].radius)
                    second = i;
            }

            return second;
        }

        private static bool PlaceSecond(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, int index, int second,
            float fraction)
        {
            float pair = context.Settings.SeparationScale
                * (planets[index].radius + planets[second].radius);
            float offset = (pair - 0.5f * planets[index].radius) / planets[second].radius;
            return PuzzleLessonPlacement.TryBeside(
                context, planets, second, fraction, -offset);
        }
    }
}
