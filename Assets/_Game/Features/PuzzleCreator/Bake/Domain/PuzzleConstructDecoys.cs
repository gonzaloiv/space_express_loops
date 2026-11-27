using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Keeps every unclaimed planet out of every ship's start corridor. A failed verdict moves the
    // decoy nearest the failing ship's chord first; the step is deterministic for a given retry seed.
    public static class PuzzleConstructDecoys
    {
        private const int Attempts = 32;

        public static void KeepClear(PuzzleDecoyField field)
        {
            SeededRandom random = new(PuzzleConstructSeeds.Value(
                field.Request.Seed.Seed, field.Request.Attempt));
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
                Vector3 candidate = Candidate(field, index, random);
                if (!Fits(field, index, candidate))
                    continue;

                field.Planets[index].position = candidate;
                return true;
            }

            return false;
        }

        // The nearest unprotected planet to the given ship's chord, or to any chord when ship < 0.
        public static int NearestUnprotected(PuzzleDecoyField field, int ship = -1)
        {
            int nearest = -1;
            float best = float.MaxValue;
            for (int i = 0; i < field.Planets.Length; i++)
            {
                if (field.Protected[i])
                    continue;

                float distance = Distance(field, i, ship);
                if (distance >= best)
                    continue;

                best = distance;
                nearest = i;
            }

            return nearest;
        }

        private static float Distance(PuzzleDecoyField field, int index, int ship)
        {
            float best = float.MaxValue;
            int count = RunCount(field);
            for (int i = 0; i < count; i++)
            {
                if (ship >= 0 && i != ship)
                    continue;

                Run(field, i, out Vector3 origin, out Vector3 destination);
                best = Mathf.Min(best, PuzzleRunGeometry.DistanceToSegment(
                    field.Planets[index].position, origin, destination));
            }

            return best;
        }

        private static bool InCorridor(PuzzleDecoyField field, int index)
        {
            float margin = field.Request.Brief.EndpointClearance
                + field.Planets[index].radius * PuzzleRunChoice.PlanetBlockBandFactor;
            int count = RunCount(field);
            for (int i = 0; i < count; i++)
            {
                Run(field, i, out Vector3 origin, out Vector3 destination);
                if (PuzzleRunGeometry.DistanceToSegment(
                        field.Planets[index].position, origin, destination) < margin)
                    return true;
            }

            return false;
        }

        // A moved decoy keeps its ring radius and height; only the azimuth changes, so a Ring or
        // Spiral field stays on its rings instead of gaining planets in the middle.
        private static Vector3 Candidate(PuzzleDecoyField field, int index, SeededRandom random)
        {
            Vector3 current = field.Planets[index].position;
            float radius = new Vector2(current.x, current.z).magnitude;
            float angle = random.Range(0f, Mathf.PI * 2f);
            return new Vector3(
                Mathf.Cos(angle) * radius, current.y, Mathf.Sin(angle) * radius);
        }

        private static bool Fits(PuzzleDecoyField field, int index, Vector3 candidate)
        {
            PuzzleFieldPlanet planet = field.Planets[index];
            float clearance = field.Request.Brief.EndpointClearance;
            float margin = clearance + planet.radius * PuzzleRunChoice.PlanetBlockBandFactor;
            int count = RunCount(field);
            for (int i = 0; i < count; i++)
            {
                Run(field, i, out Vector3 origin, out Vector3 destination);
                if (PuzzleRunGeometry.DistanceToSegment(candidate, origin, destination) < margin)
                    return false;
            }

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
            int count = RunCount(field);
            for (int i = 0; i < count; i++)
            {
                Run(field, i, out Vector3 origin, out Vector3 destination);
                if ((candidate - origin).sqrMagnitude < squared
                    || (candidate - destination).sqrMagnitude < squared)
                    return false;
            }

            return true;
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

        private static int RunCount(PuzzleDecoyField field) =>
            field.Ships != null && field.Ships.Length > 0 ? field.Ships.Length : 1;

        private static void Run(
            PuzzleDecoyField field, int index, out Vector3 origin, out Vector3 destination)
        {
            if (field.Ships != null && field.Ships.Length > 0)
            {
                PuzzleConstructedShip ship =
                    field.Ships[Mathf.Clamp(index, 0, field.Ships.Length - 1)];
                origin = ship.Origin;
                destination = ship.Destination;
                return;
            }

            origin = field.Origin;
            destination = field.Destination;
        }
    }
}
