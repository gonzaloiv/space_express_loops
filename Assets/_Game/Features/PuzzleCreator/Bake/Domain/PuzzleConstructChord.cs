using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The run's chord, picked from the generated field first: the pair closest to the target length
    // whose endpoints keep clear and whose line keeps the sun out when the brief asks. The fleet
    // picker is PuzzleConstructFleetChord; this type owns the single-chord rules both use.
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

        public static bool TryPickShips(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request,
            out PuzzleConstructedShip[] ships, out int hubIndex, out Vector3 hubHome) =>
            PuzzleConstructFleetChord.TryPickShips(
                planets, request, out ships, out hubIndex, out hubHome);

        // A pair is a run when its length and clearance fit; the fleet picker shares this rule.
        public static bool PairFits(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, int i, int j)
        {
            float length = Vector3.Distance(planets[i].position, planets[j].position);
            float target = request.Field.RunDistance;
            if (length < target * MinRunFactor || length > target * MaxRunFactor)
                return false;
            if (!EndpointsClear(planets, request.Brief.EndpointClearance, i, j))
                return false;

            return SunClear(planets[i].position, planets[j].position, request);
        }

        private static float Delta(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, int i, int j)
        {
            if (!PairFits(planets, request, i, j))
                return float.MaxValue;

            float length = Vector3.Distance(planets[i].position, planets[j].position);
            return Mathf.Abs(length - request.Field.RunDistance);
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
