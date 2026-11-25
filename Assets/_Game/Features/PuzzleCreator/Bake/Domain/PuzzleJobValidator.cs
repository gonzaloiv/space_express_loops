using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleJobValidator
    {
        public static bool Validate(
            PuzzleData puzzle, PuzzleSolutionKind kind, PuzzleZone[] solution,
            PuzzleJobContract contract, out List<string> failures, PuzzleSimSettings? settings = null,
            bool checkScramble = true)
        {
            failures = new List<string>();
            if (puzzle == null)
            {
                failures.Add("no puzzle");
                return false;
            }

            CheckBrief(puzzle, kind, contract, failures);
            CheckSources(puzzle, solution, contract, failures);
            CheckFleet(puzzle, solution, contract, failures, settings);
            CheckStartFrame(puzzle, contract, failures, settings);
            CheckSunAnswer(puzzle, contract, failures, settings);
            if (checkScramble)
                CheckScramble(puzzle, solution, contract, failures);
            return failures.Count == 0;
        }

        // The scramble bounds describe the baked answer; ShippedJobs_MeetTheContract re-solves the
        // asset, so it passes checkScramble false and judges only the frame and the ships.
        private static void CheckScramble(
            PuzzleData puzzle, PuzzleZone[] solution, PuzzleJobContract contract, List<string> failures)
        {
            if (solution == null || solution.Length == 0)
                return;

            float delta = Mathf.Abs(solution[0].scale - 1f);
            if (contract.MaxScaleDelta > 0f && delta > contract.MaxScaleDelta)
                failures.Add($"scale delta {delta}, expected at most {contract.MaxScaleDelta}");
            if (contract.MinScaleDelta > 0f && delta < contract.MinScaleDelta)
                failures.Add($"scale delta {delta}, expected at least {contract.MinScaleDelta}");
            if (contract.MinStartDistance <= 0f || !puzzle.translatable)
                return;

            if (puzzle.frames == null || puzzle.frames.Length == 0)
            {
                failures.Add("no start frame");
                return;
            }

            float distance = Vector3.Distance(puzzle.frames[0].centre, solution[0].centre);
            if (distance < contract.MinStartDistance)
                failures.Add($"start distance {distance}, expected at least {contract.MinStartDistance}");
        }

        // Needs the sim settings: only a run can tell whether a cube on the sun solves the job.
        private static void CheckSunAnswer(
            PuzzleData puzzle, PuzzleJobContract contract, List<string> failures,
            PuzzleSimSettings? settings)
        {
            if (settings.HasValue
                && contract.SunRule == PuzzleSunRule.NoSunAnswer
                && PuzzleSunRuleCheck.SunAnswers(puzzle, settings.Value))
                failures.Add("a cube on the sun solves it");
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
            if (puzzle.growOnly != contract.GrowOnly)
                failures.Add($"growOnly {puzzle.growOnly}, expected {contract.GrowOnly}");
            // A fleet's kind comes from ship 0 and an Any fleet promises no role; only a fleet that
            // asks for one (Clear) is judged on it.
            bool checkRole = contract.Ships <= 1 || contract.Mechanism != PuzzleMechanism.Any;
            if (checkRole && !PuzzleSolutionClassifier.Matches(contract.Mechanism, kind))
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
                PuzzleSunPlacementCheck.Check(sources, sun, contract.SunPlacement, failures);
            }

            if (solution == null || solution.Length == 0)
                failures.Add("no solution zones");
            else if (!PuzzleSunRuleCheck.Matches(puzzle, solution, contract.SunRule))
                failures.Add($"sun rule {contract.SunRule} failed");

            // A Clear fleet's lesson is one blocker on two or more ships' runs. Single-ship jobs and
            // Any fleets keep ship 0's count (their contracts pin MinBlockers against it).
            int blockers = contract.Ships > 1 && contract.Mechanism == PuzzleMechanism.Clear
                ? PuzzleRunBlockers.SharedCount(sources, sun, puzzle)
                : PuzzleRunBlockers.Count(sources, sun, puzzle);
            if (blockers < contract.MinBlockers)
                failures.Add($"blockers {blockers}, expected at least {contract.MinBlockers}");
        }

        private static void CheckFleet(
            PuzzleData puzzle, PuzzleZone[] solution, PuzzleJobContract contract,
            List<string> failures, PuzzleSimSettings? settings)
        {
            int expected = Mathf.Max(1, contract.Ships);
            int stored = puzzle.ships != null ? puzzle.ships.Length : 0;
            if (expected <= 1)
            {
                if (stored > 1)
                    failures.Add($"ships {stored}, expected 1");
                return;
            }

            if (stored != expected)
            {
                failures.Add($"ships {stored}, expected {expected}");
                return;
            }

            CheckRelation(puzzle.ships, contract.Relation, failures);
            if (!puzzle.translatable && settings.HasValue)
                CheckFixedFrame(puzzle, settings.Value, failures);

            if (!settings.HasValue || solution == null || solution.Length == 0)
                return;

            // Verify the passed zone for every stored ship. The validator never searches; the shipped
            // test finds the zone, this checks it. StartFails keeps the pristine run from already
            // arriving, which is what makes the edit the answer.
            PuzzleVerification check = PuzzleVerifier.Verify(puzzle, solution, settings.Value);
            if (!check.Solved || !check.StartFails)
                failures.Add("the fleet does not solve");
        }

        // A frame stores only centre and size, so a fixed fleet's frame is checked by solving it in
        // place: some reachable scale must land every ship, and the cube must keep clear of endpoints.
        private static void CheckFixedFrame(
            PuzzleData puzzle, PuzzleSimSettings settings, List<string> failures)
        {
            if (puzzle.frames == null || puzzle.frames.Length == 0)
            {
                failures.Add("no start frame");
                return;
            }

            PuzzleFrame frame = puzzle.frames[0];
            if (!PuzzleStartFrame.SolvesInPlace(puzzle, settings, frame.centre, frame.size))
                failures.Add("the fixed frame does not solve in place");
            else if (PuzzleStartFrameCheck.CoversEndpoint(puzzle))
                failures.Add("start frame covers an endpoint");
        }

        private static void CheckRelation(
            PuzzleShip[] ships, PuzzleShipRelation relation, List<string> failures)
        {
            for (int i = 0; i < ships.Length; i++)
            {
                for (int j = i + 1; j < ships.Length; j++)
                {
                    if (!PuzzleMultiShipBake.Allowed(
                            relation, ships[i].origin, ships[i].destination,
                            ships[j].origin, ships[j].destination))
                    {
                        failures.Add($"ship relation {relation} failed");
                        return;
                    }
                }
            }
        }

        // With the sim settings, the start frame may sit off the rule's spot when scaling there alone
        // would solve the job (PuzzleStartFrame); without them only the rule's spot is accepted.
        private static void CheckStartFrame(
            PuzzleData puzzle, PuzzleJobContract contract, List<string> failures,
            PuzzleSimSettings? settings)
        {
            if (puzzle.translatable)
                PuzzleStartFrameCheck.CheckMovable(puzzle, contract, settings, failures);
        }

        private static int SunIndex(PuzzleSource[] sources)
        {
            for (int i = 0; i < sources.Length; i++)
                if (sources[i].centreOnly)
                    return i;

            return -1;
        }

        // The rule a contract's SunOnChord is pinned from: whether the sun sits in the run's block band.
        public static bool SunOnChord(PuzzleData puzzle)
        {
            PuzzleSource[] sources = puzzle.sources ?? System.Array.Empty<PuzzleSource>();
            int sun = SunIndex(sources);
            return sun >= 0 && OnChord(sources[sun], puzzle);
        }

        private static bool OnChord(PuzzleSource sun, PuzzleData puzzle)
        {
            float radius = sun.scale * 0.5f;
            return PuzzleRunGeometry.DistanceToSegment(
                sun.home, puzzle.origin, puzzle.destination)
                < radius * PuzzleRunChoice.SunBlockBandFactor;
        }
    }
}
