using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleBlockerPlacement
    {
        private static readonly float[] Fractions = { 0.5f, 0.34f, 0.66f, 0.25f, 0.75f };
        private static readonly float[] WellFractions = { 0.25f, 0.75f, 0.34f, 0.66f };
        private static readonly float[] ClearOffsets = { 0.5f, -0.5f, 0.75f, -0.75f, 1f, -1f };
        private static readonly float[] CurveOffsets = { 1.5f, -1.5f, 1f, -1f, 0.5f, -0.5f };
        private static readonly float[] WellOffsets = { 0.5f, -0.5f };

        public static void Apply(
            PuzzlePlanetListAsset planets, PuzzleSeed seed, SpaceShapingSettings settings)
        {
            if (planets == null || planets.planets == null || planets.planets.Length == 0)
                return;

            int count = Count(seed, settings);
            if (count <= 0)
                return;

            PuzzleRunRequest request = Request(planets.planets, seed, settings);
            if (!PuzzleRunChoice.TryPickCandidates(
                    request, count, out PuzzleRunChoice.Candidate[] runs))
                return;

            planets.hasCandidate = true;
            planets.candidateOrigin = runs[0].Origin;
            planets.candidateDestination = runs[0].Destination;
            float[] offsets = seed.mechanism == PuzzleMechanism.Curve
                ? CurveOffsets
                : ClearOffsets;
            Place(planets.planets, count, runs, settings, offsets,
                seed.mechanism == PuzzleMechanism.Mixed, out int blockerIndex, out int wellIndex);
            planets.blockerIndex = blockerIndex;
            planets.wellIndex = wellIndex;
        }

        private static int Count(PuzzleSeed seed, SpaceShapingSettings settings)
        {
            if (seed.mechanism == PuzzleMechanism.Any
                || seed.EffectiveSunRadius(settings) <= 0f)
                return 0;

            return seed.mechanism == PuzzleMechanism.Clear
                && seed.sunRule == PuzzleSunRule.OutOfZone
                ? 2
                : 1;
        }

        private static PuzzleRunRequest Request(
            PuzzleFieldPlanet[] planets, PuzzleSeed seed, SpaceShapingSettings settings) => new()
        {
            Planets = planets,
            TargetRun = settings.runDistance,
            EndpointClearance = settings.endpointClearance,
            SunRadius = seed.EffectiveSunRadius(settings),
            RequirePlanetBlocker = false,
            AllowSunOnChord = seed.sunOnChord
        };

        private static void Place(
            PuzzleFieldPlanet[] planets, int count, PuzzleRunChoice.Candidate[] runs,
            SpaceShapingSettings settings, float[] offsets, bool placeWell,
            out int blockerIndex, out int wellIndex)
        {
            blockerIndex = -1;
            wellIndex = -1;
            bool[] used = new bool[planets.Length];
            int placed = 0;
            for (int r = 0; r < runs.Length && placed < count; r++)
            {
                if (!PlaceOne(planets, runs[r], used, settings, offsets, out int index))
                    continue;

                if (placed == 0)
                    blockerIndex = index;
                placed++;
            }

            if (placeWell && runs.Length > 0)
                wellIndex = PlaceWell(planets, runs[0], used, settings);
        }

        private static int PlaceWell(
            PuzzleFieldPlanet[] planets, PuzzleRunChoice.Candidate run, bool[] used,
            SpaceShapingSettings settings)
        {
            for (int i = 0; i < planets.Length; i++)
            {
                if (used[i] || IsEndpoint(planets[i].position, run.Origin, run.Destination))
                    continue;

                if (!TryPoint(
                        planets, i, run.Origin, run.Destination, settings,
                        WellFractions, WellOffsets, out Vector3 point))
                    continue;

                planets[i].position = point;
                used[i] = true;
                return i;
            }

            return -1;
        }

        private static bool PlaceOne(
            PuzzleFieldPlanet[] planets, PuzzleRunChoice.Candidate run, bool[] used,
            SpaceShapingSettings settings, float[] offsets, out int index)
        {
            index = -1;
            for (int i = 0; i < planets.Length; i++)
            {
                if (used[i] || IsEndpoint(planets[i].position, run.Origin, run.Destination))
                    continue;

                if (!TryPoint(
                        planets, i, run.Origin, run.Destination, settings,
                        Fractions, offsets, out Vector3 point))
                    continue;

                planets[i].position = point;
                used[i] = true;
                index = i;
                return true;
            }

            return false;
        }

        private static bool TryPoint(
            PuzzleFieldPlanet[] planets, int index, Vector3 origin, Vector3 destination,
            SpaceShapingSettings settings, float[] fractions, float[] offsets, out Vector3 point)
        {
            point = default;
            Vector3 direction = destination - origin;
            if (direction.sqrMagnitude < 0.000001f)
                return false;

            Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;
            for (int f = 0; f < fractions.Length; f++)
            {
                Vector3 centre = Vector3.Lerp(origin, destination, fractions[f]);
                for (int o = 0; o < offsets.Length; o++)
                {
                    Vector3 candidate = centre + side * (planets[index].radius * offsets[o]);
                    if (!Fits(planets, index, candidate, origin, destination, settings))
                        continue;

                    point = candidate;
                    return true;
                }
            }

            return false;
        }

        internal static bool Fits(
            PuzzleFieldPlanet[] planets, int index, Vector3 point, Vector3 origin,
            Vector3 destination, SpaceShapingSettings settings)
        {
            float radius = planets[index].radius;
            float endpoint = settings.endpointClearance + radius;
            if (Vector3.Distance(point, origin) < endpoint
                || Vector3.Distance(point, destination) < endpoint)
                return false;

            if (Vector3.Distance(point, Vector3.zero) < settings.sunRadius + radius)
                return false;

            for (int j = 0; j < planets.Length; j++)
            {
                if (j == index)
                    continue;

                float separation = (radius + planets[j].radius) * settings.separationScale;
                if (Vector3.Distance(point, planets[j].position) < separation)
                    return false;
            }

            return true;
        }

        private static bool IsEndpoint(Vector3 position, Vector3 origin, Vector3 destination) =>
            (position - origin).sqrMagnitude < 0.000001f
            || (position - destination).sqrMagnitude < 0.000001f;
    }
}
