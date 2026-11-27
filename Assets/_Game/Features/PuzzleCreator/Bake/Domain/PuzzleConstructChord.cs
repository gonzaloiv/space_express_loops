using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The run's chord, picked from the generated field first: the pair closest to the target length
    // whose endpoints keep clear and whose line keeps the sun out when the brief asks.
    public static class PuzzleConstructChord
    {
        private const float MinRunFactor = 0.5f;
        private const float MaxRunFactor = 1.5f;

        public static bool TryPick(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request,
            out int originIndex, out int destinationIndex)
        {
            originIndex = -1;
            destinationIndex = -1;
            float best = float.MaxValue;
            for (int i = 0; i < planets.Length; i++)
            {
                for (int j = i + 1; j < planets.Length; j++)
                {
                    float delta = Delta(planets, request, i, j);
                    if (delta < best)
                    {
                        best = delta;
                        originIndex = i;
                        destinationIndex = j;
                    }
                }
            }

            return originIndex >= 0;
        }

        private static float Delta(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, int i, int j)
        {
            float length = Vector3.Distance(planets[i].position, planets[j].position);
            float target = request.Field.RunDistance;
            if (length < target * MinRunFactor || length > target * MaxRunFactor)
                return float.MaxValue;
            if (!EndpointsClear(planets, request.Brief.EndpointClearance, i, j))
                return float.MaxValue;
            if (!SunClear(planets[i].position, planets[j].position, request))
                return float.MaxValue;

            return Mathf.Abs(length - target);
        }

        private static bool EndpointsClear(
            PuzzleFieldPlanet[] planets, float clearance, int i, int j)
        {
            for (int k = 0; k < planets.Length; k++)
            {
                if (k == i || k == j)
                    continue;

                float radius = clearance + planets[k].radius;
                if ((planets[k].position - planets[i].position).sqrMagnitude < radius * radius
                    || (planets[k].position - planets[j].position).sqrMagnitude < radius * radius)
                    return false;
            }

            return true;
        }

        // A Centre sun is already at the field origin when the chord is picked; an OffCorridor sun is
        // placed later, after the planets, and checked against the chord then.
        private static bool SunClear(Vector3 from, Vector3 to, PuzzleConstructRequest request)
        {
            if (request.Brief.SunRadius <= 0f
                || request.Brief.SunOnChord
                || request.Brief.SunPlacement != PuzzleSunPlacement.Centre)
                return true;

            float blocked = request.Brief.SunRadius * PuzzleRunChoice.SunBlockBandFactor;
            return PuzzleRunGeometry.DistanceToSegment(Vector3.zero, from, to) >= blocked;
        }
    }
}
