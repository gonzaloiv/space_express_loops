using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public static class PuzzleFleetSolution
    {
        private const int ScaleSamples = 32;
        private const float MergeDistance = 0.0001f;

        public static bool TryFind(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleMechanism mechanism,
            out PuzzleZone zone)
        {
            zone = default;
            if (puzzle.ships == null || puzzle.ships.Length < 2)
                return false;

            PuzzleShipScan scan = new(puzzle, settings, 0f);
            List<Vector3> centres = Centres(puzzle);
            float[] sizes = Sizes(puzzle);
            float[] scales = PuzzleZoneCandidates.Scales(settings, ScaleSamples);
            for (int c = 0; c < centres.Count; c++)
            {
                List<PuzzleZone> zones = PuzzleZoneCandidates.Build(
                    centres[c], sizes, scales, settings.ZoneFalloff);
                for (int i = 0; i < zones.Count; i++)
                {
                    if (TryZone(puzzle, settings, scan, mechanism, zones[i], out zone))
                        return true;
                }
            }

            return false;
        }

        private static bool TryZone(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleShipScan scan,
            PuzzleMechanism mechanism, PuzzleZone candidate, out PuzzleZone zone)
        {
            zone = default;
            if (!PuzzleZoneCandidates.CoversSource(candidate, puzzle)
                || PuzzleZoneCandidates.ContainsEndpoint(candidate, puzzle))
                return false;

            if (!scan.Rescues(candidate, puzzle.ships))
                return false;

            PuzzleVerification check = PuzzleVerifier.Verify(puzzle, new[] { candidate }, settings);
            if (!check.Solved)
                return false;

            // First matching zone, not the cheapest: keeping the early exit stops the shipped check
            // from verifying every candidate. Any keeps the old first-solved behaviour.
            if (mechanism != PuzzleMechanism.Any
                && !PuzzleSolutionClassifier.Matches(mechanism, check.Kind))
                return false;

            zone = candidate;
            return true;
        }

        private static List<Vector3> Centres(PuzzleData puzzle)
        {
            List<Vector3> centres = new();
            for (int i = 0; i < puzzle.sources.Length; i++)
            {
                if (!puzzle.sources[i].fixedMass)
                    Add(centres, puzzle.sources[i].home);
            }

            for (int i = 0; i < puzzle.sources.Length; i++)
            {
                for (int j = i + 1; j < puzzle.sources.Length; j++)
                {
                    if (puzzle.sources[i].fixedMass || puzzle.sources[j].fixedMass)
                        continue;

                    if (Vector3.Distance(puzzle.sources[i].home, puzzle.sources[j].home)
                        > puzzle.cubeSizeMax)
                        continue;

                    Add(centres, (puzzle.sources[i].home + puzzle.sources[j].home) * 0.5f);
                }
            }

            return centres;
        }

        private static float[] Sizes(PuzzleData puzzle)
        {
            float[] sampled = PuzzleZoneCandidates.Sizes(puzzle);
            if (puzzle.frames == null || puzzle.frames.Length == 0)
                return sampled;

            float frame = puzzle.frames[0].size;
            for (int i = 0; i < sampled.Length; i++)
            {
                if (Mathf.Abs(sampled[i] - frame) < MergeDistance)
                    return sampled;
            }

            float[] sizes = new float[sampled.Length + 1];
            System.Array.Copy(sampled, sizes, sampled.Length);
            sizes[sampled.Length] = frame;
            return sizes;
        }

        private static void Add(List<Vector3> centres, Vector3 point)
        {
            for (int i = 0; i < centres.Count; i++)
            {
                if (Vector3.Distance(centres[i], point) < MergeDistance)
                    return;
            }

            centres.Add(point);
        }
    }
}
