using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleRouteCount
    {
        public const int MinRoutes = 2;

        private const int Samples = 8;
        private const float MergeDistance = 0.0001f;

        public static int Count(PuzzleData puzzle, PuzzleSimSettings settings, PuzzleZone[] solution)
        {
            if (solution == null || solution.Length == 0)
                return 0;

            List<PuzzleZone[]> routes = new() { solution };
            for (int zone = 0; zone < solution.Length; zone++)
                AddRoutes(puzzle, settings, solution, zone, routes);

            return routes.Count;
        }

 private static void AddRoutes(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleZone[] solution, int zone,
            List<PuzzleZone[]> routes)
        {
            Vector3[] centres = PuzzleZoneCandidates.Centres(puzzle, puzzle.cubeSizeMax);
            float[] scales = PuzzleZoneCandidates.Scales(settings, Samples);
            for (int c = 0; c < centres.Length; c++)
            {
                for (int s = 0; s < scales.Length; s++)
                {
                    PuzzleZone moved = PuzzleZoneCandidates.Zone(
                        centres[c], solution[zone].Width, scales[s], settings.ZoneFalloff);
                    if (!Fits(puzzle, solution, zone, moved))
                        continue;

                    PuzzleZone[] variant = Replace(solution, zone, moved);
                    if (Arrives(puzzle, settings, variant) && !Known(routes, variant))
                        routes.Add(variant);
                }
            }
        }

 private static bool Fits(
            PuzzleData puzzle, PuzzleZone[] solution, int zone, PuzzleZone moved)
        {
            if ((moved.centre - solution[zone].centre).sqrMagnitude < MergeDistance * MergeDistance)
                return false;

            if (PuzzleZoneCandidates.ContainsEndpoint(moved, puzzle)
                || !PuzzleZoneCandidates.CoversSource(moved, puzzle))
                return false;

            for (int i = 0; i < solution.Length; i++)
            {
                if (i != zone && moved.Overlaps(solution[i], PuzzleZoneCandidates.ZoneGapFactor))
                    return false;
            }

            return true;
        }

        private static bool Arrives(PuzzleData puzzle, PuzzleSimSettings settings, PuzzleZone[] zones) =>
            PuzzleZoneRun.TryArrive(puzzle, settings, zones, out PuzzleSolution solution)
            && solution.EntropySpent <= puzzle.entropyBudget;

        private static PuzzleZone[] Replace(PuzzleZone[] zones, int index, PuzzleZone value)
        {
            PuzzleZone[] next = new PuzzleZone[zones.Length];
            for (int i = 0; i < zones.Length; i++)
                next[i] = i == index ? value : zones[i];

            return next;
        }

        private static bool Known(List<PuzzleZone[]> routes, PuzzleZone[] variant)
        {
            for (int i = 0; i < routes.Count; i++)
            {
                if (Same(routes[i], variant))
                    return true;
            }

            return false;
        }

        private static bool Same(PuzzleZone[] a, PuzzleZone[] b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if ((a[i].centre - b[i].centre).sqrMagnitude >= MergeDistance * MergeDistance)
                    return false;
            }

            return true;
        }
    }
}
