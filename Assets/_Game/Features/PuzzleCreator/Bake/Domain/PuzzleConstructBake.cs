using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Materializes a constructed field on the main thread and runs the bake's whole gate: the answer
    // lands, the start fails, the brief and sun rules hold, the routes exist, the contract passes, and
    // the solve window is not a knife edge.
    public static class PuzzleConstructBake
    {
        public const int DefaultAttempts = 4;
        private const float OpenBudget = 1e9f;

        public static PuzzleBakeResult Bake(
            PuzzleConstructRequest request, int attempts = DefaultAttempts)
        {
            PuzzleBakeResult last = default;
            int count = Mathf.Max(1, attempts);
            for (int attempt = 0; attempt < count; attempt++)
            {
                last = TryAttempt(request.WithAttempt(attempt), attempt);
                if (last.Baked)
                    return last;
            }

            return last;
        }

        // A local decoy move stays within the attempt; the next attempt re-seeds the stream from
        // (seed, attempt), so an attempt-1 retry is not the next seed.
        private static PuzzleBakeResult TryAttempt(
            PuzzleConstructRequest request, int attempt)
        {
            PuzzleConstructedField field =
                PuzzleConstruct.Build(request, out PuzzleConstructStep failedStep);
            if (field == null)
                return NoRun(failedStep);

            PuzzleBakeResult result = Verify(field, request);
            if (result.Baked)
                return result;

            field = PuzzleConstruct.MoveDecoy(field, request, attempt + 1);
            return field != null ? Verify(field, request) : result;
        }

        public static PuzzleBakeResult Verify(
            PuzzleConstructedField field, PuzzleConstructRequest request)
        {
            PuzzleData puzzle = Materialize(field, request);
            try
            {
                return VerifyAnswer(puzzle, field.Answer, request);
            }
            finally
            {
                Object.DestroyImmediate(puzzle);
            }
        }

        public static PuzzleBakeResult VerifyAnswer(
            PuzzleData field, PuzzleZone[] answer, PuzzleConstructRequest request)
        {
            PuzzleData copy = Object.Instantiate(field);
            try
            {
                return Judge(copy, answer, request);
            }
            finally
            {
                Object.DestroyImmediate(copy);
            }
        }

        private static PuzzleBakeResult Judge(
            PuzzleData puzzle, PuzzleZone[] answer, PuzzleConstructRequest request)
        {
            if (answer == null || answer.Length == 0)
                return Fail(PuzzleBakeStatus.NoSolution);

            puzzle.entropyBudget = OpenBudget;
            PuzzleVerification baseline = PuzzleVerifier.Verify(puzzle, answer, request.Sim);
            if (!baseline.Solved)
                return Fail(PuzzleBakeStatus.Unverified, baseline.Kind);

            PuzzleSolution found = new()
            {
                Zones = answer,
                Kind = baseline.Kind,
                EntropySpent = baseline.EntropySpent
            };
            return Finish(
                PuzzleFieldBake.Check(puzzle, request.Sim, request.Brief, found, baseline.Kind),
                request);
        }

        private static PuzzleBakeResult Finish(PuzzleBakeResult result, PuzzleConstructRequest request)
        {
            if (!result.Baked)
                return result;
            if (!PuzzleJobValidator.Validate(
                    result.Puzzle, result.Kind, result.Solution, request.Contract,
                    out List<string> problems, request.Sim))
                return Failed(result, PuzzleBakeStatus.ContractFailed, problems);
            if (ClearShortcut(result.Puzzle, request))
                return Failed(result, PuzzleBakeStatus.ClearShortcut);

            float window = PuzzleSolveWindow.Measure(result.Puzzle, result.Solution, request.Sim);
            if (window <= 0f)
                return Failed(result, PuzzleBakeStatus.Unverified);

            result.Window = window;
            return result;
        }

        private static PuzzleBakeResult Failed(
            PuzzleBakeResult result, PuzzleBakeStatus status, List<string> problems = null)
        {
            Object.DestroyImmediate(result.Puzzle);
            PuzzleBakeResult failed = Fail(status, result.Kind);
            failed.Problems = problems;
            return failed;
        }

        // A Curve job must not be solvable by shrinking alone: a cube that shrinks a blocker and lands
        // is a Clear shortcut, not the bend lesson. Fleets share one cube across every blocker; a
        // movable single-ship job can put the cube on the blocker, a fixed one cannot.
        public static bool ClearShortcut(PuzzleData puzzle, PuzzleConstructRequest request)
        {
            if (request.Brief.Mechanism != PuzzleMechanism.Curve)
                return false;
            if (request.Brief.IsMulti)
                return PuzzleFleetSolution.TryFind(
                    puzzle, request.Sim, PuzzleMechanism.Clear, out _);
            if (request.Brief.GrowOnly || !request.Brief.Translatable)
                return false;

            PuzzleBrief clear = request.Brief;
            clear.Mechanism = PuzzleMechanism.Clear;
            return PuzzleSolutionSearch.TrySelect(puzzle, request.Sim, clear, out _);
        }

        private static PuzzleData Materialize(
            PuzzleConstructedField field, PuzzleConstructRequest request)
        {
            PuzzleFieldPlanet[] planets = (PuzzleFieldPlanet[])field.Planets.Clone();
            PlanetNames names = PlanetNames.Instance;
            names?.Assign(planets);
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.id = request.Contract.Id;
            puzzle.sources = Sources(planets, field);
            puzzle.origin = field.Origin;
            puzzle.destination = field.Destination;
            puzzle.cubeSizeMin = field.CubeSizeMin;
            puzzle.cubeSizeMax = field.CubeSizeMax;
            ApplyBrief(puzzle, request.Brief, field.Answer);
            puzzle.entropyBudget = OpenBudget;
            puzzle.ships = Fleet(field) ? ToShips(field.Ships) : null;
            puzzle.zoneCentre = Fleet(field)
                ? PuzzleMultiShipBake.Centre(puzzle.ships)
                : (field.Origin + field.Destination) * 0.5f;
            puzzle.captureRadius = field.Ships != null && field.Ships.Length > 0
                ? field.Ships[0].CaptureRadius
                : puzzle.CaptureRadiusAtDestination();
            return puzzle;
        }

        private static bool Fleet(PuzzleConstructedField field) =>
            field.Ships != null && field.Ships.Length > 1;

        private static PuzzleShip[] ToShips(PuzzleConstructedShip[] ships)
        {
            PuzzleShip[] result = new PuzzleShip[ships.Length];
            for (int i = 0; i < ships.Length; i++)
            {
                result[i] = new PuzzleShip
                {
                    origin = ships[i].Origin,
                    destination = ships[i].Destination,
                    captureRadius = ships[i].CaptureRadius
                };
            }

            return result;
        }

        private static void ApplyBrief(
            PuzzleData puzzle, PuzzleBrief brief, PuzzleZone[] answer)
        {
            puzzle.translatable = brief.Translatable;
            puzzle.zones = answer != null && answer.Length > 0 ? answer.Length : brief.Zones;
            puzzle.resizable = brief.Resizable;
            puzzle.noFail = brief.NoFail;
            puzzle.noGrade = brief.NoGrade;
            puzzle.noIntro = brief.NoIntro;
            puzzle.growOnly = brief.GrowOnly;
        }

        private static PuzzleSource[] Sources(
            PuzzleFieldPlanet[] planets, PuzzleConstructedField field)
        {
            PuzzleSource[] planetSources = PuzzleSourceBridge.ToSources(planets);
            return field.HasSun
                ? PuzzleSunSources.WithSun(
                    planetSources, field.SunRadius, field.SunScalable, field.SunHome)
                : planetSources;
        }

        private static PuzzleBakeResult NoRun(PuzzleConstructStep step)
        {
            PuzzleBakeResult failed = Fail(PuzzleBakeStatus.NoRun);
            failed.Problems = new List<string> { $"build step: {step}" };
            return failed;
        }

        private static PuzzleBakeResult Fail(
            PuzzleBakeStatus status, PuzzleSolutionKind kind = PuzzleSolutionKind.Other) =>
            new() { Status = status, Kind = kind };
    }
}
