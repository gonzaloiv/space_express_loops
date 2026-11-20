using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSunPlacementChoice
    {
        private const int Directions = 32;
        private const float RimFactor = 1.05f;

        public static bool TryHome(
            PuzzleFieldPlanet[] planets, float sunRadius, float clearance, out Vector3 home)
        {
            home = Vector3.zero;
            if (planets == null || planets.Length == 0 || sunRadius <= 0f)
                return false;

            float radius = FieldRadius(planets) * RimFactor + sunRadius + clearance;
            float best = float.MinValue;
            for (int i = 0; i < Directions; i++)
            {
                Vector3 candidate = Direction(i) * radius;
                if (!ClearOfPlanets(candidate, planets, sunRadius + clearance))
                    continue;

                float score = ClearanceFromRuns(candidate, planets);
                if (score <= best)
                    continue;

                best = score;
                home = candidate;
            }

            return best > float.MinValue;
        }

        private static float FieldRadius(PuzzleFieldPlanet[] planets)
        {
            float radius = 0f;
            for (int i = 0; i < planets.Length; i++)
                radius = Mathf.Max(radius, planets[i].position.magnitude);

            return radius;
        }

        private static bool ClearOfPlanets(
            Vector3 candidate, PuzzleFieldPlanet[] planets, float clearance)
        {
            for (int i = 0; i < planets.Length; i++)
            {
                if (Vector3.Distance(candidate, planets[i].position)
                    < planets[i].radius + clearance)
                    return false;
            }

            return true;
        }

        private static float ClearanceFromRuns(Vector3 candidate, PuzzleFieldPlanet[] planets)
        {
            float clearance = float.MaxValue;
            for (int i = 0; i < planets.Length; i++)
            {
                for (int j = i + 1; j < planets.Length; j++)
                {
                    float distance = PuzzleRunGeometry.DistanceToSegment(
                        candidate, planets[i].position, planets[j].position);
                    clearance = Mathf.Min(clearance, distance);
                }
            }

            return clearance;
        }

        private static Vector3 Direction(int index)
        {
            float y = 1f - 2f * (index + 0.5f) / Directions;
            float ring = Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));
            float angle = index * 2.39996323f;
            return new Vector3(Mathf.Cos(angle) * ring, y, Mathf.Sin(angle) * ring);
        }
    }
}
