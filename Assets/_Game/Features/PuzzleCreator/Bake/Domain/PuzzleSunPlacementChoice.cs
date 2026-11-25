using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSunPlacementChoice
    {
        private const int Azimuths = 32;
        private const int BandHeights = 3;
        private const float RimFactor = 1.05f;

        public static bool TryHome(
            PuzzleFieldPlanet[] planets, float sunRadius, float clearance, out Vector3 home)
        {
            home = Vector3.zero;
            if (planets == null || planets.Length == 0 || sunRadius <= 0f)
                return false;

            float radius = FieldRadius(planets) * RimFactor + sunRadius + clearance;
            float low = float.MaxValue;
            float high = float.MinValue;
            for (int i = 0; i < planets.Length; i++)
            {
                low = Mathf.Min(low, planets[i].position.y);
                high = Mathf.Max(high, planets[i].position.y);
            }

            float best = float.MinValue;
            for (int h = 0; h < BandHeights; h++)
            {
                float y = BandHeights == 1
                    ? (low + high) * 0.5f
                    : Mathf.Lerp(low, high, h / (BandHeights - 1f));
                float horizontal = Mathf.Sqrt(Mathf.Max(0f, radius * radius - y * y));
                for (int a = 0; a < Azimuths; a++)
                {
                    float angle = a * (2f * Mathf.PI / Azimuths);
                    Vector3 candidate = new(
                        Mathf.Cos(angle) * horizontal, y, Mathf.Sin(angle) * horizontal);
                    if (!ClearOfPlanets(candidate, planets, sunRadius + clearance))
                        continue;

                    float score = ClearanceFromRuns(candidate, planets);
                    if (score <= best)
                        continue;

                    best = score;
                    home = candidate;
                }
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
    }
}
