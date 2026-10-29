using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    internal static class PuzzleRunChoice
    {
        private const float MinRunFraction = 0.5f;
        private const float MaxRunFactor = 1.5f;
        private const float BlockBandFactor = 2f;

        public static bool TryPick(
            PuzzleFieldPlanet[] planets, float targetRun, float endpointClearance,
            out Vector3 origin, out Vector3 destination)
        {
            RunChoice choice = new() { Best = float.MaxValue };
            RunTarget target = new(targetRun, endpointClearance);
            for (int i = 0; i < planets.Length; i++)
            {
                for (int j = i + 1; j < planets.Length; j++)
                    Consider(planets, i, j, target, ref choice);
            }

            origin = choice.Origin;
            destination = choice.Destination;
            return choice.Found;
        }

        private static void Consider(
            PuzzleFieldPlanet[] planets, int i, int j, RunTarget target, ref RunChoice choice)
        {
            float run = Vector3.Distance(planets[i].position, planets[j].position);
            if (run < target.Distance * MinRunFraction || run > target.Distance * MaxRunFactor)
                return;

            if (!RunIsBlocked(planets, i, j, target.EndpointClearance))
                return;

            float delta = Mathf.Abs(run - target.Distance);
            if (delta >= choice.Best)
                return;

            choice.Best = delta;
            choice.Origin = planets[i].position;
            choice.Destination = planets[j].position;
            choice.Found = true;
        }

        private static bool RunIsBlocked(
            PuzzleFieldPlanet[] planets, int a, int b, float endpointClearance)
        {
            Vector3 from = planets[a].position;
            Vector3 to = planets[b].position;
            bool blocked = false;
            for (int k = 0; k < planets.Length; k++)
            {
                if (k == a || k == b)
                    continue;

                Vector3 home = planets[k].position;
                float radius = planets[k].radius;
                if (Vector3.Distance(home, from) < endpointClearance + radius
                    || Vector3.Distance(home, to) < endpointClearance + radius)
                    return false;

                if (PuzzleRunGeometry.DistanceToSegment(home, from, to) < radius * BlockBandFactor)
                    blocked = true;
            }

            return blocked;
        }

        private readonly struct RunTarget
        {
            public readonly float Distance;
            public readonly float EndpointClearance;

            public RunTarget(float distance, float endpointClearance)
            {
                Distance = distance;
                EndpointClearance = endpointClearance;
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
