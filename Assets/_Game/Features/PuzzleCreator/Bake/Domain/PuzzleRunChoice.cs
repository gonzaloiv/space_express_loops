using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleRunChoice
    {
        private const float MinRunFraction = 0.5f;
        private const float MaxRunFactor = 1.5f;
        internal const float BlockBandFactor = 2f;

        public static bool TryPick(
            PuzzleRunRequest request, out Vector3 origin, out Vector3 destination)
        {
            RunChoice choice = new() { Best = float.MaxValue };
            for (int i = 0; i < request.Planets.Length; i++)
            {
                for (int j = i + 1; j < request.Planets.Length; j++)
                    Consider(i, j, request, ref choice);
            }

            origin = choice.Origin;
            destination = choice.Destination;
            return choice.Found;
        }

        private static void Consider(
            int i, int j, PuzzleRunRequest request, ref RunChoice choice)
        {
            Vector3 from = request.Planets[i].position;
            Vector3 to = request.Planets[j].position;
            float run = Vector3.Distance(from, to);
            if (run < request.TargetRun * MinRunFraction || run > request.TargetRun * MaxRunFactor)
                return;

            RunBlockers blockers = Blockers(request, i, j);
            if (!blockers.Valid)
                return;
            if (request.RequirePlanetBlocker && !blockers.Planet)
                return;
            if (!request.AllowSunOnChord && blockers.Sun)
                return;
            if (!blockers.Planet && !blockers.Sun)
                return;

            float delta = Mathf.Abs(run - request.TargetRun);
            if (delta >= choice.Best)
                return;

            choice.Best = delta;
            choice.Origin = from;
            choice.Destination = to;
            choice.Found = true;
        }

        private static RunBlockers Blockers(
            PuzzleRunRequest request, int a, int b)
        {
            Vector3 from = request.Planets[a].position;
            Vector3 to = request.Planets[b].position;
            if (!EndpointsClear(request, a, b))
                return default;

            bool sun = SunBlocked(request, from, to);
            bool planet = PlanetBlocked(request, a, b, from, to);
            return new RunBlockers(true, planet, sun);
        }

        private static bool EndpointsClear(
            PuzzleRunRequest request, int a, int b)
        {
            Vector3 from = request.Planets[a].position;
            Vector3 to = request.Planets[b].position;
            if (request.SunRadius > 0f
                && (Vector3.Distance(Vector3.zero, from) < request.EndpointClearance + request.SunRadius
                    || Vector3.Distance(Vector3.zero, to) < request.EndpointClearance + request.SunRadius))
                return false;

            for (int k = 0; k < request.Planets.Length; k++)
            {
                if (k == a || k == b)
                    continue;

                float radius = request.EndpointClearance + request.Planets[k].radius;
                if (Vector3.Distance(request.Planets[k].position, from) < radius
                    || Vector3.Distance(request.Planets[k].position, to) < radius)
                    return false;
            }

            return true;
        }

        private static bool SunBlocked(
            PuzzleRunRequest request, Vector3 from, Vector3 to) =>
            request.SunRadius > 0f
            && PuzzleRunGeometry.DistanceToSegment(Vector3.zero, from, to)
                < request.SunRadius * BlockBandFactor;

        private static bool PlanetBlocked(
            PuzzleRunRequest request, int a, int b, Vector3 from, Vector3 to)
        {
            for (int k = 0; k < request.Planets.Length; k++)
            {
                if (k == a || k == b)
                    continue;

                float radius = request.Planets[k].radius;
                if (PuzzleRunGeometry.DistanceToSegment(
                        request.Planets[k].position, from, to)
                    < radius * BlockBandFactor)
                    return true;
            }

            return false;
        }

        private readonly struct RunBlockers
        {
            public readonly bool Valid;
            public readonly bool Planet;
            public readonly bool Sun;

            public RunBlockers(bool valid, bool planet, bool sun)
            {
                Valid = valid;
                Planet = planet;
                Sun = sun;
            }
        }

        private struct RunChoice
        {
            public Vector3 Origin;
            public Vector3 Destination;
            public float Best;
            public bool Found;
        }
    }
}
