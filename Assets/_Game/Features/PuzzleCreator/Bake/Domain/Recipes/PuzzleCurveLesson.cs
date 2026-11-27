using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public sealed class PuzzleCurveLesson : IPuzzleLesson
    {
        private static readonly float[] BlockFractions = { 0.5f, 0.34f, 0.66f };
        private static readonly float[] BendFractions = { 0.5f, 0.66f, 0.34f };
        private static readonly float[] Offsets = { 1.5f, -1.5f, 1f, -1f, 0.5f, -0.5f };

        public PuzzleLessonPlan Place(PuzzleLessonContext context)
        {
            int[] order = Order(context, true);
            for (int i = 0; i < order.Length; i++)
            {
                int blocker = order[i];
                for (int f = 0; f < BlockFractions.Length; f++)
                {
                    if (!TryBlocker(
                            context, blocker, BlockFractions[f], out PuzzleFieldPlanet[] planets))
                        continue;

                    PuzzleLessonPlan plan = TryBend(context, planets, blocker);
                    if (plan != null)
                        return plan;
                }
            }

            return null;
        }

        private static bool TryBlocker(
            PuzzleLessonContext context, int index, float fraction,
            out PuzzleFieldPlanet[] planets)
        {
            planets = (PuzzleFieldPlanet[])context.Planets.Clone();
            return PuzzleLessonPlacement.TryOnChord(context, planets, index, fraction);
        }

        private static PuzzleLessonPlan TryBend(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, int blocker)
        {
            int[] order = Order(context, false);
            for (int i = 0; i < order.Length; i++)
            {
                int bend = order[i];
                if (bend == blocker)
                    continue;

                PuzzleLessonPlan plan = TryBendAt(context, planets, blocker, bend);
                if (plan != null)
                    return plan;
            }

            return null;
        }

        private static int[] Order(PuzzleLessonContext context, bool descending)
        {
            int[] order = new int[context.Planets.Length];
            bool[] used = new bool[order.Length];
            int count = 0;
            for (int i = 0; i < order.Length; i++)
            {
                int best = Pick(context, used, descending);
                if (best < 0)
                    break;

                used[best] = true;
                order[count] = best;
                count++;
            }

            return Trim(order, count);
        }

        private static int Pick(PuzzleLessonContext context, bool[] used, bool descending)
        {
            int best = -1;
            for (int i = 0; i < context.Planets.Length; i++)
            {
                if (used[i] || IsEndpoint(context, i))
                    continue;
                if (best < 0 || Better(context, i, best, descending))
                    best = i;
            }

            return best;
        }

        private static int[] Trim(int[] order, int count)
        {
            int[] trimmed = new int[count];
            for (int i = 0; i < count; i++)
                trimmed[i] = order[i];
            return trimmed;
        }

        private static bool Better(
            PuzzleLessonContext context, int index, int best, bool descending)
        {
            float radius = context.Planets[index].radius;
            float other = context.Planets[best].radius;
            return descending ? radius > other : radius < other;
        }

        private static bool IsEndpoint(PuzzleLessonContext context, int index) =>
            index == context.OriginIndex || index == context.DestinationIndex;

        private static PuzzleLessonPlan TryBendAt(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, int blocker, int bend)
        {
            for (int f = 0; f < BendFractions.Length; f++)
            {
                for (int o = 0; o < Offsets.Length; o++)
                {
                    PuzzleFieldPlanet[] candidate =
                        (PuzzleFieldPlanet[])planets.Clone();
                    if (!PuzzleLessonPlacement.TryBeside(
                            context, candidate, bend, BendFractions[f], Offsets[o]))
                        continue;

                    PuzzleLessonPlan plan = Plan(context, candidate, blocker, bend);
                    if (plan != null)
                        return plan;
                }
            }

            return null;
        }

        private static PuzzleLessonPlan Plan(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, int blocker, int bend)
        {
            SpaceShapingSetup setup = PuzzleLessonRun.Setup(planets, context);
            if (!PuzzleLessonRun.StartsAsTheBriefAsks(setup, context, out _))
                return null;
            if (ShrinkSolved(context, setup, planets, blocker))
                return null;
            if (!TryAnswer(context, planets, bend, setup, out PuzzleZone answer))
                return null;

            return new PuzzleLessonPlan(
                Moves(planets, blocker, bend), new[] { answer }, PuzzleSolutionKind.Curve);
        }

        private static bool TryAnswer(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, int bend,
            SpaceShapingSetup setup, out PuzzleZone answer)
        {
            answer = default;
            float radius = planets[bend].radius;
            PuzzleZone zone = PuzzleZoneCandidates.Zone(
                planets[bend].position, PuzzleLessonAnswer.Size(context, radius),
                1f, context.Settings.ZoneFalloff);
            if (PuzzleLessonAnswer.Rejected(context, zone))
                return false;

            return PuzzleLessonScale.TryMiddle(
                setup, zone, 1f, context.Settings.MaxScaleFactor, out answer);
        }

        private static PuzzleLessonMove[] Moves(
            PuzzleFieldPlanet[] planets, int blocker, int bend) => new[]
        {
            new PuzzleLessonMove(blocker, planets[blocker].position, planets[blocker].radius),
            new PuzzleLessonMove(bend, planets[bend].position, planets[bend].radius)
        };

        private static bool ShrinkSolved(
            PuzzleLessonContext context, SpaceShapingSetup setup,
            PuzzleFieldPlanet[] planets, int blocker)
        {
            if (!context.Brief.Translatable || context.Brief.GrowOnly)
                return false;

            PuzzleZone zone = PuzzleZoneCandidates.Zone(
                planets[blocker].position,
                PuzzleLessonAnswer.Size(context, planets[blocker].radius),
                1f, context.Settings.ZoneFalloff);
            return PuzzleLessonScale.AnyArrives(
                setup, zone, context.Settings.MinScaleFactor, 1f);
        }
    }
}
