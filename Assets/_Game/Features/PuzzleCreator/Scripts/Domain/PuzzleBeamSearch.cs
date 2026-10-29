using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    internal static class PuzzleBeamSearch
    {
        private const int Width = 8;
        private const int MaxEvaluations = 5000;
        private const float MarginEpsilon = 0.0001f;
        private const float SizeTolerance = 0.0001f;

        public static bool TrySelect(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleBrief brief, out PuzzleSolution solution) =>
            TrySelect(puzzle, settings, brief, 0f, out solution);

        public static bool TrySelect(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleBrief brief, float width,
            out PuzzleSolution solution)
        {
            solution = default;
            int moves = Mathf.Clamp(brief.Moves, 2, 3);
            PuzzleSearchContext context = new(
                puzzle, settings,
                PuzzleZoneCandidates.Centres(puzzle, brief.MaxCubeSize),
                PuzzleZoneCandidates.Scales(settings, moves == 2 ? 8 : 6));

            List<PuzzleChain> beam = FirstStep(
                context, PuzzleZoneCandidates.Sizes(puzzle), width);
            for (int depth = 2; depth <= moves && beam.Count > 0; depth++)
                beam = Expand(context, beam, depth == moves);

            return Choose(beam, puzzle, out solution);
        }

        private static List<PuzzleChain> FirstStep(
            PuzzleSearchContext context, float[] sizes, float width)
        {
            List<PuzzleChain> beam = new();
            for (int c = 0; c < context.Centres.Length; c++)
            {
                for (int s = 0; s < sizes.Length; s++)
                {
                    if (width > 0f && Mathf.Abs(sizes[s] - width) > SizeTolerance)
                        continue;

                    AddBest(context, context.Centres[c], sizes[s], beam);
                }
            }

            return beam;
        }

        private static void AddBest(
            PuzzleSearchContext context, Vector3 centre, float size, List<PuzzleChain> beam)
        {
            PuzzleChain best = default;
            bool found = false;
            for (int i = 0; i < context.Scales.Length; i++)
            {
                if (!TryCandidate(context, centre, size, context.Scales[i], out PuzzleChain chain))
                    continue;

                if (!found || Precedes(chain, best, context.Puzzle, true))
                {
                    best = chain;
                    found = true;
                }
            }

            if (found)
                beam.Add(best);
        }

        private static bool TryCandidate(
            PuzzleSearchContext context, Vector3 centre, float size, float scale, out PuzzleChain chain)
        {
            chain = default;
            if (context.Evaluations >= MaxEvaluations)
                return false;

            PuzzleZone zone = PuzzleZoneCandidates.Zone(centre, size, scale, context.Settings.ZoneFalloff);
            if (PuzzleZoneCandidates.ContainsEndpoint(zone, context.Puzzle))
                return false;

            if (!PuzzleZoneCandidates.ContainsSource(zone, context.Puzzle))
                return false;

            context.Evaluations++;
            chain = Evaluate(context, new[] { zone });
            return true;
        }

        private static List<PuzzleChain> Expand(
            PuzzleSearchContext context, List<PuzzleChain> beam, bool final)
        {
            List<PuzzleChain> next = new();
            for (int b = 0; b < beam.Count; b++)
                ExpandChain(context, next, beam[b], final);

            return Ordered(next, context.Puzzle, final);
        }

        private static void ExpandChain(
            PuzzleSearchContext context, List<PuzzleChain> next, PuzzleChain prefix, bool final)
        {
            for (int c = 0; c < context.Centres.Length; c++)
            {
                for (int i = 0; i < context.Scales.Length; i++)
                {
                    PuzzleZone zone = PuzzleZoneCandidates.Zone(
                        context.Centres[c], prefix.Size, context.Scales[i], context.Settings.ZoneFalloff);
                    AddExpansion(context, next, prefix, zone, final);
                }
            }
        }

        private static void AddExpansion(
            PuzzleSearchContext context, List<PuzzleChain> next, PuzzleChain prefix, PuzzleZone zone,
            bool final)
        {
            if (context.Evaluations >= MaxEvaluations)
                return;

            if (PuzzleZoneCandidates.ContainsEndpoint(zone, context.Puzzle)
                || !PuzzleZoneCandidates.ContainsSource(zone, context.Puzzle)
                || Overlaps(prefix.Zones, zone))
                return;

            context.Evaluations++;
            PuzzleChain candidate = Evaluate(context, Append(prefix.Zones, zone));
            if (final && (!candidate.Arrived
                || !PuzzleWindow.Contains(context.Puzzle, candidate.Solution.ShipTime)))
                return;

            next.Add(candidate);
        }

        private static PuzzleChain Evaluate(PuzzleSearchContext context, PuzzleZone[] zones)
        {
            PuzzleZoneRun.Simulate(
                context.Puzzle, context.Settings, zones,
                out PuzzleSolution solution, out TrajectoryResult result);

            return new PuzzleChain(
                zones, solution, Margin(result, context.Puzzle.destination),
                result.Outcome == TrajectoryOutcome.Arrive);
        }

        private static bool Choose(List<PuzzleChain> beam, PuzzleData puzzle, out PuzzleSolution solution)
        {
            solution = default;
            PuzzleChain best = default;
            bool found = false;
            for (int i = 0; i < beam.Count; i++)
            {
                PuzzleChain chain = beam[i];
                if (!chain.Arrived || !PuzzleWindow.Contains(puzzle, chain.Solution.ShipTime))
                    continue;

                if (!found || Precedes(chain, best, puzzle, false))
                {
                    best = chain;
                    found = true;
                }
            }

            if (found)
                solution = best.Solution;

            return found;
        }

        private static int Order(PuzzleChain a, PuzzleChain b, PuzzleData puzzle, bool final)
        {
            if (!final && a.Margin != b.Margin)
                return a.Margin < b.Margin ? -1 : 1;

            return PuzzleSolutionOrdering.Order(
                a.Solution, b.Solution, PuzzleSolutionOrdering.WindowCentre(puzzle));
        }

        private static bool Precedes(PuzzleChain a, PuzzleChain b, PuzzleData puzzle, bool marginFirst)
        {
            if (marginFirst && !SameMargin(a, b))
                return a.Margin < b.Margin;

            return PuzzleSolutionOrdering.Precedes(
                a.Solution, b.Solution, PuzzleSolutionOrdering.WindowCentre(puzzle));
        }

        private static bool SameMargin(PuzzleChain a, PuzzleChain b) =>
            Mathf.Abs(a.Margin - b.Margin) <= MarginEpsilon;

        private static PuzzleZone[] Append(PuzzleZone[] zones, PuzzleZone zone)
        {
            PuzzleZone[] next = new PuzzleZone[zones.Length + 1];
            for (int i = 0; i < zones.Length; i++)
                next[i] = zones[i];

            next[zones.Length] = zone;
            return next;
        }

        private static bool Overlaps(PuzzleZone[] zones, PuzzleZone zone)
        {
            for (int i = 0; i < zones.Length; i++)
            {
                if (zone.Overlaps(zones[i], PuzzleZoneCandidates.ZoneGapFactor))
                    return true;
            }

            return false;
        }

        private static List<PuzzleChain> Ordered(List<PuzzleChain> chains, PuzzleData puzzle, bool final)
        {
            chains.Sort((a, b) => Order(a, b, puzzle, final));
            return Trim(chains);
        }

        private static List<PuzzleChain> Trim(List<PuzzleChain> chains)
        {
            if (chains.Count <= Width)
                return chains;

            List<PuzzleChain> trimmed = new(Width);
            for (int i = 0; i < Width; i++)
                trimmed.Add(chains[i]);

            return trimmed;
        }

        private static float Margin(TrajectoryResult result, Vector3 destination) =>
            result.HasPoints ? result.ClosestApproach(destination) : float.MaxValue;
    }
}
