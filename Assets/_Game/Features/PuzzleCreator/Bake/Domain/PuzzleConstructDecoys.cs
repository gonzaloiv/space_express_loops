using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Keeps every unclaimed planet out of the start corridor. A failed verdict moves the decoy
    // nearest the chord first; the step is deterministic for a given retry seed.
    public static class PuzzleConstructDecoys
    {
        private const int Attempts = 32;

        public static void KeepClear(PuzzleDecoyField field)
        {
            SeededRandom random = new(
                field.Request.Seed.Seed + field.Request.Attempt * 7919);
            for (int i = 0; i < field.Planets.Length; i++)
            {
                if (field.Protected[i] || !InCorridor(field, i))
                    continue;

                TryRelocate(field, i, random);
            }
        }

        public static bool TryRelocate(PuzzleDecoyField field, int index, SeededRandom random)
        {
            for (int i = 0; i < Attempts; i++)
            {
                Vector3 candidate = Candidate(field, random);
                if (!Fits(field, index, candidate))
                    continue;

                field.Planets[index].position = candidate;
                return true;
            }

            return false;
        }

        public static int NearestUnprotected(PuzzleDecoyField field)
        {
            int nearest = -1;
            float best = float.MaxValue;
            for (int i = 0; i < field.Planets.Length; i++)
            {
                if (field.Protected[i])
                    continue;

                float distance = PuzzleRunGeometry.DistanceToSegment(
                    field.Planets[i].position, field.Origin, field.Destination);
                if (distance >= best)
                    continue;

                best = distance;
                nearest = i;
            }

            return nearest;
        }

        private static bool InCorridor(PuzzleDecoyField field, int index)
        {
            float margin = field.Request.Brief.EndpointClearance
                + field.Planets[index].radius * PuzzleRunChoice.PlanetBlockBandFactor;
            return PuzzleRunGeometry.DistanceToSegment(
                field.Planets[index].position, field.Origin, field.Destination) < margin;
        }

        private static Vector3 Candidate(PuzzleDecoyField field, SeededRandom random)
        {
            Vector3 point = random.InsideCircleXZ(field.Radius);
            point.y = random.Range(-1f, 1f)
                * field.Radius * field.Request.Field.FieldThickness;
            return point;
        }

        private static bool Fits(PuzzleDecoyField field, int index, Vector3 candidate)
        {
            PuzzleFieldPlanet planet = field.Planets[index];
            float clearance = field.Request.Brief.EndpointClearance;
            float margin = clearance + planet.radius * PuzzleRunChoice.PlanetBlockBandFactor;
            if (PuzzleRunGeometry.DistanceToSegment(candidate, field.Origin, field.Destination) < margin)
                return false;
            if (!ClearOfEndpoints(field, candidate, clearance + planet.radius))
                return false;
            if (!Separated(field, index, candidate))
                return false;
            return ClearOfSun(field, candidate, planet.radius);
        }

        private static bool ClearOfEndpoints(
            PuzzleDecoyField field, Vector3 candidate, float clearance)
        {
            float squared = clearance * clearance;
            return (candidate - field.Origin).sqrMagnitude >= squared
                && (candidate - field.Destination).sqrMagnitude >= squared;
        }

        private static bool Separated(PuzzleDecoyField field, int index, Vector3 candidate)
        {
            float scale = field.Request.Sim.SeparationScale;
            for (int i = 0; i < field.Planets.Length; i++)
            {
                if (i == index)
                    continue;

                float required = scale * (field.Planets[index].radius + field.Planets[i].radius);
                if ((candidate - field.Planets[i].position).sqrMagnitude < required * required)
                    return false;
            }

            return true;
        }

        private static bool ClearOfSun(PuzzleDecoyField field, Vector3 candidate, float radius)
        {
            float sun = field.Request.Brief.SunRadius;
            if (sun <= 0f || field.Request.Brief.SunPlacement != PuzzleSunPlacement.Centre)
                return true;

            float required = sun + radius;
            return candidate.sqrMagnitude >= required * required;
        }
    }
}
