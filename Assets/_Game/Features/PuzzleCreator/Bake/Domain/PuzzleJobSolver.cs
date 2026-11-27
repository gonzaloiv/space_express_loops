using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The one way a shipped asset's answer is found: the shipped check used to own this, and the
    // answer backfill needs the same search. A fleet walks its solved candidates and returns the first
    // that meets the contract, so the stored answer passes the shipped check including its bounds.
    public static class PuzzleJobSolver
    {
        public sealed class AnswerAttempt
        {
            public bool Found;
            public PuzzleSolution Solution;
            public readonly List<string> Problems = new();
        }

        public static AnswerAttempt TrySolveAnswer(
            PuzzleData puzzle, PuzzleJobContract contract, SpaceShapingSettings settings,
            PuzzleSimSettings sim)
        {
            AnswerAttempt attempt = new();
            attempt.Found = contract.Ships > 1
                ? TrySolveFleet(puzzle, contract, sim, attempt)
                : TrySolveSingle(puzzle, contract, Brief(contract, settings), sim, attempt);
            return attempt;
        }

        private static bool TrySolveSingle(
            PuzzleData puzzle, PuzzleJobContract contract, PuzzleBrief brief,
            PuzzleSimSettings sim, AnswerAttempt attempt)
        {
            PuzzleSolution solution = default;
            bool found = puzzle.translatable
                ? PuzzleSolutionSearch.TrySelect(puzzle, sim, brief, out solution)
                : PuzzleSolutionSearch.TryFindAtFrame(puzzle, sim, brief, out solution);
            if (!found)
            {
                attempt.Problems.Add("no solution found");
                return false;
            }

            if (!PuzzleJobValidator.Validate(
                    puzzle, solution.Kind, solution.Zones, contract,
                    out List<string> failures, sim))
            {
                attempt.Problems.AddRange(failures);
                return false;
            }

            attempt.Solution = solution;
            return true;
        }

        private static bool TrySolveFleet(
            PuzzleData puzzle, PuzzleJobContract contract, PuzzleSimSettings sim,
            AnswerAttempt attempt)
        {
            if (!puzzle.translatable && !CheckFixedOnce(puzzle, contract, sim, attempt))
                return false;

            FleetAnswer answer = new(puzzle, contract, sim, attempt);
            if (PuzzleFleetSolution.TryFind(
                    puzzle, sim, contract.Mechanism, answer.Accept, out PuzzleZone found))
            {
                attempt.Solution = Solution(found, answer.Verification);
                return true;
            }

            if (attempt.Problems.Count == 0)
                attempt.Problems.Add("no solved zone");
            return false;
        }

        // A fixed fleet's frame checks are zone-independent, so run them once instead of on every
        // candidate. A movable fleet keeps the full validation per candidate.
        private static bool CheckFixedOnce(
            PuzzleData puzzle, PuzzleJobContract contract, PuzzleSimSettings sim,
            AnswerAttempt attempt)
        {
            PuzzleJobValidator.CheckFixedFrame(puzzle, sim, attempt.Problems);
            PuzzleJobValidator.CheckSunAnswer(puzzle, contract, attempt.Problems, sim);
            return attempt.Problems.Count == 0;
        }

        private static PuzzleSolution Solution(PuzzleZone zone, PuzzleVerification check) =>
            new()
            {
                Zones = new[] { zone },
                EntropySpent = check.EntropySpent,
                Kind = check.Kind
            };

        private static PuzzleBrief Brief(PuzzleJobContract contract, SpaceShapingSettings settings)
        {
            PuzzleSeed seed = contract.CreateSeed();
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            Object.DestroyImmediate(seed);
            return brief;
        }

        private sealed class FleetAnswer
        {
            private readonly PuzzleData puzzle;
            private readonly PuzzleJobContract contract;
            private readonly PuzzleSimSettings sim;
            private readonly AnswerAttempt attempt;

            public FleetAnswer(
                PuzzleData puzzle, PuzzleJobContract contract, PuzzleSimSettings sim,
                AnswerAttempt attempt)
            {
                this.puzzle = puzzle;
                this.contract = contract;
                this.sim = sim;
                this.attempt = attempt;
            }

            public PuzzleVerification Verification { get; private set; }

            public bool Accept(PuzzleZone zone, PuzzleVerification verification)
            {
                if (!verification.Solved || !verification.StartFails)
                    return false;

                PuzzleSimSettings? settings = puzzle.translatable ? sim : null;
                if (!PuzzleJobValidator.Validate(
                        puzzle, verification.Kind, new[] { zone }, contract,
                        out List<string> failures, settings))
                {
                    if (attempt.Problems.Count == 0)
                        attempt.Problems.AddRange(failures);
                    return false;
                }

                Verification = verification;
                return true;
            }
        }
    }
}
