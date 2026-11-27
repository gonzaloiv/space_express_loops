using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // One scalable blocker near each chord and one shared bend beside the common region; the answer
    // grows the bend. The blockers stay full, so the starts fail without the bend edit.
    public sealed class PuzzleFleetCurveLesson : IPuzzleLesson
    {
        private static readonly float[] BlockFractions = { 0.5f, 0.34f, 0.66f };
        private static readonly float[] Offsets = { 1.5f, -1.5f, 1f, -1f, 0.5f, -0.5f };

        public PuzzleLessonPlan Place(PuzzleLessonContext context)
        {
            if (context.HubIndex < 0 || context.Ships == null || context.Ships.Length < 2)
                return null;

            PuzzleLessonMove[] blockers = Blockers(context);
            return blockers != null ? Bend(context, blockers) : null;
        }

        private static PuzzleLessonMove[] Blockers(PuzzleLessonContext context)
        {
            PuzzleFieldPlanet[] planets = (PuzzleFieldPlanet[])context.Planets.Clone();
            bool[] used = Used(context);
            PuzzleLessonMove[] moves = new PuzzleLessonMove[context.Ships.Length];
            for (int ship = 0; ship < context.Ships.Length; ship++)
            {
                if (!PlaceBlocker(context, planets, used, context.Ships[ship], out moves[ship]))
                    return null;
            }

            return moves;
        }

        private static bool PlaceBlocker(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, bool[] used,
            PuzzleConstructedShip ship, out PuzzleLessonMove move)
        {
            move = default;
            context.Chord = ship;
            try
            {
                for (int i = 0; i < planets.Length; i++)
                {
                    if (used[i])
                        continue;
                    if (BlockerAt(context, planets, used, i, out move))
                        return true;
                }
            }
            finally
            {
                context.Chord = null;
            }

            return false;
        }

        private static bool BlockerAt(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, bool[] used, int index,
            out PuzzleLessonMove move)
        {
            move = default;
            for (int f = 0; f < BlockFractions.Length; f++)
            {
                PuzzleFieldPlanet[] candidate = (PuzzleFieldPlanet[])planets.Clone();
                if (!PuzzleLessonPlacement.TryOnChord(context, candidate, index, BlockFractions[f]))
                    continue;

                planets[index] = candidate[index];
                used[index] = true;
                move = new PuzzleLessonMove(index, planets[index].position, planets[index].radius);
                return true;
            }

            return false;
        }

        private static PuzzleLessonPlan Bend(
            PuzzleLessonContext context, PuzzleLessonMove[] blockers)
        {
            context.Chord = context.Ships[0];
            try
            {
                for (int o = 0; o < Offsets.Length; o++)
                {
                    PuzzleFieldPlanet[] planets = Copy(context.Planets, blockers);
                    if (!BendAt(context, planets, Offsets[o]))
                        continue;

                    PuzzleLessonPlan plan = Plan(context, planets, blockers);
                    if (plan != null)
                        return plan;
                }
            }
            finally
            {
                context.Chord = null;
            }

            return null;
        }

        // The bend is the shared hub point, offset to one side of the first chord.
        private static bool BendAt(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, float offset)
        {
            Vector3 direction = context.Chord.Destination - context.Chord.Origin;
            if (direction.sqrMagnitude <= 0f)
                return false;

            Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;
            float radius = planets[context.HubIndex].radius;
            Vector3 home = context.HubPosition + side * (radius * offset);
            return PuzzleLessonPlacement.TryAt(context, planets, context.HubIndex, home);
        }

        private static PuzzleLessonPlan Plan(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, PuzzleLessonMove[] blockers)
        {
            SpaceShapingSetup setup = PuzzleLessonRun.Setup(planets, context);
            TrajectoryResult[] starts = PuzzleLessonRun.StartResults(setup);
            if (!PuzzleLessonRun.StartsAsTheBriefAsks(starts, context))
                return null;
            if (!PuzzleLessonRun.StartsFailForEveryShip(starts))
                return null;

            float radius = planets[context.HubIndex].radius;
            PuzzleZone zone = PuzzleZoneCandidates.Zone(
                planets[context.HubIndex].position, PuzzleLessonAnswer.Size(context, radius), 1f,
                context.Settings.ZoneFalloff);
            if (PuzzleLessonAnswer.Rejected(context, zone) || HoldsAnother(context, planets, zone))
                return null;
            if (!PuzzleLessonScale.TryMiddle(
                    setup, zone, 1f, context.Settings.MaxScaleFactor, out PuzzleZone answer))
                return null;

            return new PuzzleLessonPlan(
                Moves(context, planets, blockers), new[] { answer }, PuzzleSolutionKind.Curve);
        }

        private static PuzzleLessonMove[] Moves(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, PuzzleLessonMove[] blockers)
        {
            PuzzleLessonMove[] moves = new PuzzleLessonMove[blockers.Length + 1];
            for (int i = 0; i < blockers.Length; i++)
                moves[i] = blockers[i];

            PuzzleFieldPlanet hub = planets[context.HubIndex];
            moves[blockers.Length] = new PuzzleLessonMove(
                context.HubIndex, hub.position, hub.radius);
            return moves;
        }

        private static bool[] Used(PuzzleLessonContext context)
        {
            bool[] used = new bool[context.Planets.Length];
            used[context.HubIndex] = true;
            for (int i = 0; i < context.Ships.Length; i++)
            {
                used[context.Ships[i].OriginIndex] = true;
                used[context.Ships[i].DestinationIndex] = true;
            }

            return used;
        }

        private static PuzzleFieldPlanet[] Copy(
            PuzzleFieldPlanet[] planets, PuzzleLessonMove[] moves)
        {
            PuzzleFieldPlanet[] copy = (PuzzleFieldPlanet[])planets.Clone();
            for (int i = 0; i < moves.Length; i++)
                copy[moves[i].Index] = new PuzzleFieldPlanet(
                    moves[i].Home, moves[i].Radius, copy[moves[i].Index].name);
            return copy;
        }

        private static bool HoldsAnother(
            PuzzleLessonContext context, PuzzleFieldPlanet[] planets, PuzzleZone zone)
        {
            for (int i = 0; i < planets.Length; i++)
            {
                if (i != context.HubIndex && zone.Contains(planets[i].position))
                    return true;
            }

            return false;
        }
    }
}
