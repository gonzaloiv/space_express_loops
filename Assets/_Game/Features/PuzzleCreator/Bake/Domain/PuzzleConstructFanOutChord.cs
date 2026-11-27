using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // A fan-out's chords: one shared origin planet, one destination per ship inside a 90° cone, and a
    // free bend near the origin, clear of every endpoint. The aim step then moves the destinations.
    public static class PuzzleConstructFanOutChord
    {
        public const float ConeDegrees = 90f;

        public static bool TryPick(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request,
            out PuzzleConstructedShip[] ships, out int hubIndex)
        {
            ships = null;
            hubIndex = -1;
            int count = request.Brief.Ships;
            if (count < 2 || count >= planets.Length)
                return false;

            return TryFrom(planets, request, Groups(planets, request, count), out ships, out hubIndex);
        }

        private static List<Group> Groups(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, int count)
        {
            List<Group> groups = new();
            for (int origin = 0; origin < planets.Length; origin++)
            {
                int[] candidates = Candidates(planets, request, origin);
                if (candidates.Length < count)
                    continue;

                Collect(planets, request, origin, candidates, new int[count], 0, 0, groups);
            }

            return groups;
        }

        private static bool TryFrom(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, List<Group> groups,
            out PuzzleConstructedShip[] ships, out int hubIndex)
        {
            groups.Sort((a, b) => a.Error.CompareTo(b.Error));
            for (int i = 0; i < groups.Count; i++)
            {
                int bend = Bend(planets, request, groups[i]);
                if (bend < 0)
                    continue;

                ships = Ships(planets, groups[i]);
                hubIndex = bend;
                return true;
            }

            ships = null;
            hubIndex = -1;
            return false;
        }

        // The fan's angle: every pair of destinations seen from the shared origin stays inside the cone.
        private static bool InsideCone(
            PuzzleFieldPlanet[] planets, int origin, int[] chosen, int taken)
        {
            Vector3 direction = planets[chosen[taken]].position - planets[origin].position;
            for (int i = 0; i < taken; i++)
            {
                Vector3 other = planets[chosen[i]].position - planets[origin].position;
                if (Vector3.Angle(direction, other) > ConeDegrees)
                    return false;
            }

            return true;
        }

        private static int[] Candidates(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, int origin)
        {
            List<int> candidates = new();
            for (int i = 0; i < planets.Length; i++)
            {
                if (i != origin && PuzzleConstructChord.PairFits(planets, request, origin, i))
                    candidates.Add(i);
            }

            return candidates.ToArray();
        }

        private static void Collect(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, int origin,
            int[] candidates, int[] chosen, int taken, int start, List<Group> groups)
        {
            if (taken == chosen.Length)
            {
                groups.Add(new Group
                {
                    Origin = origin,
                    Destinations = (int[])chosen.Clone(),
                    Error = Error(planets, request, origin, chosen)
                });
                return;
            }

            for (int i = start; i <= candidates.Length - (chosen.Length - taken); i++)
            {
                chosen[taken] = candidates[i];
                if (!InsideCone(planets, origin, chosen, taken))
                    continue;

                Collect(planets, request, origin, candidates, chosen, taken + 1, i + 1, groups);
            }
        }

        private static float Error(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, int origin, int[] chosen)
        {
            float error = 0f;
            for (int i = 0; i < chosen.Length; i++)
            {
                error += Mathf.Abs(
                    Vector3.Distance(planets[chosen[i]].position, planets[origin].position)
                    - request.Field.RunDistance);
            }

            return error;
        }

        private static int Bend(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, Group group)
        {
            List<int> free = Free(planets, group);
            free.Sort((a, b) => Distance(planets, a, group.Origin)
                .CompareTo(Distance(planets, b, group.Origin)));
            for (int i = 0; i < free.Count; i++)
            {
                if (Clears(planets, request, group, free[i]))
                    return free[i];
            }

            return -1;
        }

        private static bool Clears(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, Group group, int index)
        {
            PuzzleConstructedShip[] ships = Ships(planets, group);
            PuzzleLessonContext context = PuzzleConstructLesson.Context(
                planets, request, ships, index, planets[index].position);
            PuzzleFieldPlanet[] copy = (PuzzleFieldPlanet[])planets.Clone();
            return PuzzleLessonPlacement.TryAt(context, copy, index, planets[index].position);
        }

        private static PuzzleConstructedShip[] Ships(
            PuzzleFieldPlanet[] planets, Group group)
        {
            PuzzleConstructedShip[] ships = new PuzzleConstructedShip[group.Destinations.Length];
            for (int i = 0; i < ships.Length; i++)
            {
                int destination = group.Destinations[i];
                ships[i] = new PuzzleConstructedShip
                {
                    OriginIndex = group.Origin,
                    DestinationIndex = destination,
                    Origin = planets[group.Origin].position,
                    Destination = planets[destination].position,
                    CaptureRadius = planets[destination].radius
                };
            }

            return ships;
        }

        private static List<int> Free(PuzzleFieldPlanet[] planets, Group group)
        {
            List<int> free = new();
            for (int i = 0; i < planets.Length; i++)
            {
                if (i == group.Origin || System.Array.IndexOf(group.Destinations, i) >= 0)
                    continue;

                free.Add(i);
            }

            return free;
        }

        private static float Distance(PuzzleFieldPlanet[] planets, int index, int origin) =>
            Vector3.Distance(planets[index].position, planets[origin].position);

        private struct Group
        {
            public int Origin;
            public int[] Destinations;
            public float Error;
        }
    }
}
