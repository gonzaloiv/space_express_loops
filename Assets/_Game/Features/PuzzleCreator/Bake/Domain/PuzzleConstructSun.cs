using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The sun goes on last: Centre is the field origin, OffCorridor is the public rim choice, which
    // needs the planets to derive its band and rim.
    public static class PuzzleConstructSun
    {
        public static bool TryPlace(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request,
            out Vector3 home, out bool hasSun)
        {
            float radius = request.Brief.SunRadius;
            home = Vector3.zero;
            hasSun = radius > 0f;
            if (!hasSun)
                return true;
            if (request.Brief.SunPlacement == PuzzleSunPlacement.Centre)
                return true;

            return PuzzleSunPlacementChoice.TryHome(
                planets, radius, request.Brief.EndpointClearance, out home);
        }

        // An OffCorridor sun is placed after the chord; an OffCorridor contract that forbids a sun on
        // the chord must still pass the validator's SunOnChord rule.
        public static bool ClearsChord(
            Vector3 home, Vector3 origin, Vector3 destination, PuzzleConstructRequest request)
        {
            if (request.Brief.SunOnChord || request.Brief.SunRadius <= 0f)
                return true;
            if (request.Brief.SunPlacement == PuzzleSunPlacement.Centre)
                return true;

            float blocked = request.Brief.SunRadius * PuzzleRunChoice.SunBlockBandFactor;
            return PuzzleRunGeometry.DistanceToSegment(home, origin, destination) >= blocked;
        }
    }
}
