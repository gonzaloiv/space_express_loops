using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleZoneCandidates
    {
        public const int DefaultSamples = 32;
        public const int CoarseSamples = 24;
        public const float ZoneGapFactor = 0.25f;
        public const float PlanetMargin = 1.1f;

        private const int SizeSamples = 3;
        private const int MaxCentres = 8;
        private const float MergeDistance = 0.0001f;

        public static Vector3[] Centres(PuzzleData puzzle, float maxCubeSize)
        {
            List<Vector3> centres = new();
            AddSources(centres, puzzle.sources);
            AddPairs(centres, puzzle.sources, maxCubeSize);
            centres.Sort((a, b) => RunDistance(a, puzzle).CompareTo(RunDistance(b, puzzle)));
            return Trim(centres, MaxCentres);
        }

        // A zone only counts if it covers a scaleable source: a planet's centre must sit inside the zone
        // and the zone must enclose its body with a margin, so the cube reads as wrapping the planet. The
        // sun is centre-only — a cube over its centre scales it without needing to enclose the star. A
        // fixed source is skipped: it never scales, so it must not demand a star-sized cube.
        public static bool CoversSource(PuzzleZone zone, PuzzleData puzzle)
        {
            bool covered = false;
            for (int i = 0; i < puzzle.sources.Length; i++)
            {
                if (puzzle.sources[i].fixedMass)
                    continue;

                if (!zone.Contains(puzzle.sources[i].home))
                    continue;

                if (!puzzle.sources[i].centreOnly
                    && zone.Extent < puzzle.sources[i].scale * 0.5f * PlanetMargin)
                    return false;

                covered = true;
            }

            return covered;
        }

        public static bool ContainsEndpoint(PuzzleZone zone, PuzzleData puzzle) =>
            zone.Influences(puzzle.origin) || zone.Influences(puzzle.destination);

        public static float[] Sizes(PuzzleData puzzle)
        {
            float min = Mathf.Max(puzzle.cubeSizeMin, 0.01f);
            float max = Mathf.Max(puzzle.cubeSizeMax, min);
            if (max - min < MergeDistance)
                return new[] { min };

            float[] sizes = new float[SizeSamples];
            for (int i = 0; i < sizes.Length; i++)
                sizes[i] = Mathf.Lerp(min, max, i / (float)(sizes.Length - 1));

            return sizes;
        }

        public static float[] Scales(PuzzleSimSettings settings, int count)
        {
            float[] scales = new float[count];
            for (int i = 0; i < count; i++)
                scales[i] = Mathf.Lerp(settings.MinScaleFactor, settings.MaxScaleFactor, i / (float)(count - 1));

            System.Array.Sort(scales, (a, b) => Mathf.Abs(a - 1f).CompareTo(Mathf.Abs(b - 1f)));
            return scales;
        }

        public static PuzzleZone Zone(Vector3 centre, float size, float scale, float falloff) => new()
        {
            centre = centre,
            halfExtents = Vector3.one * (size * 0.5f),
            scale = scale,
            falloff = falloff
        };

        public static List<PuzzleZone> Build(
            Vector3[] centres, float[] sizes, float[] scales, float falloff)
        {
            List<PuzzleZone> zones = new(centres.Length * sizes.Length * scales.Length);
            for (int c = 0; c < centres.Length; c++)
            {
                for (int s = 0; s < sizes.Length; s++)
                {
                    for (int k = 0; k < scales.Length; k++)
                        zones.Add(Zone(centres[c], sizes[s], scales[k], falloff));
                }
            }

            return zones;
        }

        public static List<PuzzleZone> Build(
            Vector3 centre, float[] sizes, float[] scales, float falloff) =>
            Build(new[] { centre }, sizes, scales, falloff);

        private static void AddSources(List<Vector3> centres, PuzzleSource[] sources)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i].fixedMass)
                    continue;

                Add(centres, sources[i].home);
            }
        }

        private static void AddPairs(List<Vector3> centres, PuzzleSource[] sources, float maxCubeSize)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                for (int j = i + 1; j < sources.Length; j++)
                {
                    if (sources[i].fixedMass || sources[j].fixedMass)
                        continue;

                    AddPair(centres, sources[i].home, sources[j].home, maxCubeSize);
                }
            }
        }

        private static void AddPair(List<Vector3> centres, Vector3 a, Vector3 b, float maxCubeSize)
        {
            if (Vector3.Distance(a, b) > maxCubeSize)
                return;

            Add(centres, (a + b) * 0.5f);
        }

        private static void Add(List<Vector3> centres, Vector3 centre)
        {
            for (int i = 0; i < centres.Count; i++)
            {
                if (Vector3.Distance(centres[i], centre) < MergeDistance)
                    return;
            }

            centres.Add(centre);
        }

        private static float RunDistance(Vector3 centre, PuzzleData puzzle) =>
            PuzzleRunGeometry.DistanceToSegment(centre, puzzle.origin, puzzle.destination);

        private static Vector3[] Trim(List<Vector3> centres, int max)
        {
            int count = Mathf.Min(centres.Count, max);
            Vector3[] trimmed = new Vector3[count];
            for (int i = 0; i < count; i++)
                trimmed[i] = centres[i];

            return trimmed;
        }
    }
}
