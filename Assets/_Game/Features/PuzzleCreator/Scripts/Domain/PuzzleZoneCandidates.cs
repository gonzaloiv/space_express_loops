using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleZoneCandidates
    {
        public const int DefaultSamples = 32;
        public const int CoarseSamples = 24;
        public const float ZoneGapFactor = 0.25f;

        private const int SizeSamples = 3;
        private const int MaxCentres = 8;
        private const float MergeDistance = 0.0001f;
        private const float OffPathFactor = 0.35f;

        public static Vector3[] Centres(PuzzleData puzzle, float maxCubeSize)
        {
            List<Vector3> centres = new();
            AddSources(centres, puzzle.sources);
            AddPairs(centres, puzzle.sources, maxCubeSize);
            centres.Sort((a, b) => RunDistance(a, puzzle).CompareTo(RunDistance(b, puzzle)));
            return Trim(centres, MaxCentres);
        }

        public static Vector3[] ClockCentres(PuzzleData puzzle)
        {
            float run = Vector3.Distance(puzzle.origin, puzzle.destination);
            float minDistance = OffPathFactor * run;
            List<Vector3> centres = new();
            for (int i = 0; i < puzzle.sources.Length; i++)
            {
                Vector3 home = puzzle.sources[i].home;
                if (IsEndpoint(home, puzzle))
                    continue;

                float distance = PuzzleRunGeometry.DistanceToSegment(
                    home, puzzle.origin, puzzle.destination);
                if (distance < minDistance)
                    continue;

                Add(centres, home);
            }

            return centres.ToArray();
        }

        internal static bool ContainsSource(PuzzleZone zone, PuzzleData puzzle)
        {
            for (int i = 0; i < puzzle.sources.Length; i++)
            {
                if (zone.Contains(puzzle.sources[i].home))
                    return true;
            }

            return false;
        }

        public static bool ContainsEndpoint(PuzzleZone zone, PuzzleData puzzle) =>
            zone.Influences(puzzle.origin) || zone.Influences(puzzle.destination);

        public static bool ContainsOnlySource(PuzzleZone zone, PuzzleData puzzle, Vector3 target)
        {
            int inside = 0;
            for (int i = 0; i < puzzle.sources.Length; i++)
            {
                if (!zone.Contains(puzzle.sources[i].home))
                    continue;

                if (Vector3.Distance(puzzle.sources[i].home, target) > MergeDistance)
                    return false;

                inside++;
            }

            return inside == 1;
        }

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

        private static void AddSources(List<Vector3> centres, PuzzleSource[] sources)
        {
            for (int i = 0; i < sources.Length; i++)
                Add(centres, sources[i].home);
        }

        private static void AddPairs(List<Vector3> centres, PuzzleSource[] sources, float maxCubeSize)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                for (int j = i + 1; j < sources.Length; j++)
                    AddPair(centres, sources[i].home, sources[j].home, maxCubeSize);
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

        private static bool IsEndpoint(Vector3 home, PuzzleData puzzle) =>
            Vector3.Distance(home, puzzle.origin) < MergeDistance
            || Vector3.Distance(home, puzzle.destination) < MergeDistance;

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
