using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleJobValidator
    {
        public static bool Validate(
            PuzzleData puzzle, PuzzleSolutionKind kind, PuzzleZone[] solution,
            PuzzleJobContract contract, out List<string> failures)
        {
            failures = new List<string>();
            if (puzzle == null)
            {
                failures.Add("no puzzle");
                return false;
            }

            CheckBrief(puzzle, kind, contract, failures);
            CheckSources(puzzle, solution, contract, failures);
            CheckStartFrame(puzzle, contract, failures);
            return failures.Count == 0;
        }

        private static void CheckBrief(
            PuzzleData puzzle, PuzzleSolutionKind kind, PuzzleJobContract contract, List<string> failures)
        {
            if (puzzle.zones != contract.Zones)
                failures.Add($"zones {puzzle.zones}, expected {contract.Zones}");
            if (puzzle.translatable != contract.Translatable)
                failures.Add($"translatable {puzzle.translatable}, expected {contract.Translatable}");
            if (puzzle.noFail != contract.NoFail)
                failures.Add($"noFail {puzzle.noFail}, expected {contract.NoFail}");
            if (puzzle.noGrade != contract.NoGrade)
                failures.Add($"noGrade {puzzle.noGrade}, expected {contract.NoGrade}");
            if (puzzle.noIntro != contract.NoIntro)
                failures.Add($"noIntro {puzzle.noIntro}, expected {contract.NoIntro}");
            if (!PuzzleSolutionClassifier.Matches(contract.Mechanism, kind))
                failures.Add($"role {kind}, expected {contract.Mechanism}");
            if (puzzle.routes < contract.MinRoutes)
                failures.Add($"routes {puzzle.routes}, expected at least {contract.MinRoutes}");
        }

        private static void CheckSources(
            PuzzleData puzzle, PuzzleZone[] solution, PuzzleJobContract contract, List<string> failures)
        {
            PuzzleSource[] sources = puzzle.sources ?? System.Array.Empty<PuzzleSource>();
            int sun = SunIndex(sources);
            bool hasSun = sun >= 0;
            if (hasSun != contract.HasSun)
                failures.Add($"sun {hasSun}, expected {contract.HasSun}");

            int planets = sources.Length - (hasSun ? 1 : 0);
            if (planets != contract.PlanetCount)
                failures.Add($"planets {planets}, expected {contract.PlanetCount}");

            if (hasSun)
            {
                bool onChord = OnChord(sources[sun], puzzle);
                if (onChord != contract.SunOnChord)
                    failures.Add($"sun on chord {onChord}, expected {contract.SunOnChord}");
            }

            if (solution == null || solution.Length == 0)
                failures.Add("no solution zones");
            else if (!PuzzleSunRuleCheck.Matches(puzzle, solution, contract.SunRule))
                failures.Add($"sun rule {contract.SunRule} failed");

            int blockers = BlockerCount(sources, sun, puzzle);
            if (blockers < contract.MinBlockers)
                failures.Add($"blockers {blockers}, expected at least {contract.MinBlockers}");
        }

        private static void CheckStartFrame(
            PuzzleData puzzle, PuzzleJobContract contract, List<string> failures)
        {
            if (!puzzle.translatable)
                return;

            if (puzzle.frames == null || puzzle.frames.Length == 0)
            {
                failures.Add("no start frame");
                return;
            }

            Vector3 expected = contract.StartRule == PuzzleStartRule.FieldCentre
                ? Vector3.zero
                : puzzle.zoneCentre;
            if ((puzzle.frames[0].centre - expected).sqrMagnitude > 0.000001f)
                failures.Add($"start frame {puzzle.frames[0].centre}, expected {expected}");
            else if (StartFrameCoversEndpoint(puzzle))
                failures.Add("start frame covers an endpoint");
        }

        private static bool StartFrameCoversEndpoint(PuzzleData puzzle)
        {
            PuzzleZone frame = new()
            {
                centre = puzzle.frames[0].centre,
                halfExtents = puzzle.frames[0].BoxSize * 0.5f,
                scale = 1f
            };
            PuzzleEndpoints endpoints = puzzle.Endpoints;
            for (int i = 0; i < endpoints.Count; i++)
            {
                if (frame.Contains(endpoints[i]))
                    return true;
            }

            return false;
        }

        private static int SunIndex(PuzzleSource[] sources)
        {
            for (int i = 0; i < sources.Length; i++)
                if (sources[i].centreOnly)
                    return i;

            return -1;
        }

        private static bool OnChord(PuzzleSource sun, PuzzleData puzzle)
        {
            float radius = sun.scale * 0.5f;
            return PuzzleRunGeometry.DistanceToSegment(
                sun.home, puzzle.origin, puzzle.destination)
                < radius * PuzzleRunChoice.SunBlockBandFactor;
        }

        private static int BlockerCount(PuzzleSource[] sources, int sun, PuzzleData puzzle)
        {
            int count = 0;
            for (int i = 0; i < sources.Length; i++)
            {
                if (i == sun || IsEndpoint(sources[i].home, puzzle))
                    continue;

                float radius = sources[i].scale * 0.5f;
                if (PuzzleRunGeometry.DistanceToSegment(
                        sources[i].home, puzzle.origin, puzzle.destination)
                    < radius * PuzzleRunChoice.PlanetBlockBandFactor)
                    count++;
            }

            return count;
        }

        private static bool IsEndpoint(Vector3 home, PuzzleData puzzle) =>
            (home - puzzle.origin).sqrMagnitude < 0.000001f
            || (home - puzzle.destination).sqrMagnitude < 0.000001f;
    }
}
