using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSunPlacementCheck
    {
        private const float OriginTolerance = 0.0001f;
        private const float BandMargin = 0.01f;

        public static void Check(
            PuzzleSource[] sources, int sun, PuzzleSunPlacement placement, List<string> failures)
        {
            if (placement == PuzzleSunPlacement.Centre)
            {
                CheckCentred(sources[sun].home, failures);
                return;
            }

            CheckOffCorridor(sources, sun, failures);
        }

        private static void CheckCentred(Vector3 home, List<string> failures)
        {
            if (home.sqrMagnitude > OriginTolerance * OriginTolerance)
                failures.Add($"sun placement Centre: {home}, expected the field origin");
        }

        private static void CheckOffCorridor(
            PuzzleSource[] sources, int sun, List<string> failures)
        {
            if (!TryPlanetBand(sources, sun, out float low, out float high, out float rim))
                return;

            Vector3 home = sources[sun].home;
            if (home.y < low - BandMargin || home.y > high + BandMargin)
                failures.Add($"sun placement OffCorridor: y {home.y}, expected {low} to {high}");

            float horizontal = Mathf.Sqrt(home.x * home.x + home.z * home.z);
            if (horizontal <= rim)
                failures.Add(
                    $"sun placement OffCorridor: horizontal {horizontal}, expected beyond {rim}");
        }

        private static bool TryPlanetBand(
            PuzzleSource[] sources, int sun, out float low, out float high, out float rim)
        {
            low = float.MaxValue;
            high = float.MinValue;
            rim = 0f;
            for (int i = 0; i < sources.Length; i++)
            {
                if (i == sun)
                    continue;

                Vector3 planet = sources[i].home;
                low = Mathf.Min(low, planet.y);
                high = Mathf.Max(high, planet.y);
                rim = Mathf.Max(rim, Mathf.Sqrt(planet.x * planet.x + planet.z * planet.z));
            }

            return low <= high;
        }
    }
}
