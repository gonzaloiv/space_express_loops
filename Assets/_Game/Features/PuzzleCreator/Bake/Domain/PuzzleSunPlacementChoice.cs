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
            float rim = HorizontalReach(planets);
            PlanetBand(planets, out float low, out float high);
            float best = float.MinValue;
            for (int h = 0; h < BandHeights; h++)
            {
                float height = Mathf.Lerp(low, high, h / (BandHeights - 1f));
                TryBand(
                    planets, new SunBand(radius, rim, height, sunRadius + clearance),
                    ref best, ref home);
            }

            return best > float.MinValue;
        }

        private static void TryBand(
            PuzzleFieldPlanet[] planets, SunBand band, ref float best, ref Vector3 home)
        {
            float squared = band.Radius * band.Radius - band.Height * band.Height;
            float sphere = Mathf.Sqrt(Mathf.Max(0f, squared));
            float horizontal = sphere < band.Rim ? band.Radius : sphere;
            for (int a = 0; a < Azimuths; a++)
            {
                float angle = a * (2f * Mathf.PI / Azimuths);
                Vector3 candidate = new(
                    Mathf.Cos(angle) * horizontal, band.Height, Mathf.Sin(angle) * horizontal);
                if (!ClearOfPlanets(candidate, planets, band.Clearance))
                    continue;

                float score = ClearanceFromRuns(candidate, planets);
                if (score <= best)
                    continue;

                best = score;
                home = candidate;
            }
        }

        private static void PlanetBand(
            PuzzleFieldPlanet[] planets, out float low, out float high)
        {
            low = float.MaxValue;
            high = float.MinValue;
            for (int i = 0; i < planets.Length; i++)
            {
                low = Mathf.Min(low, planets[i].position.y);
                high = Mathf.Max(high, planets[i].position.y);
            }
        }

        private readonly struct SunBand
        {
            public readonly float Radius;
            public readonly float Rim;
            public readonly float Height;
            public readonly float Clearance;

            public SunBand(float radius, float rim, float height, float clearance)
            {
                Radius = radius;
                Rim = rim;
                Height = height;
                Clearance = clearance;
            }
        }

        private static float FieldRadius(PuzzleFieldPlanet[] planets)
        {
            float radius = 0f;
            for (int i = 0; i < planets.Length; i++)
                radius = Mathf.Max(radius, planets[i].position.magnitude);

            return radius;
        }

        private static float HorizontalReach(PuzzleFieldPlanet[] planets)
        {
            float reach = 0f;
            for (int i = 0; i < planets.Length; i++)
            {
                float x = planets[i].position.x;
                float z = planets[i].position.z;
                reach = Mathf.Max(reach, Mathf.Sqrt(x * x + z * z));
            }

            return reach;
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
