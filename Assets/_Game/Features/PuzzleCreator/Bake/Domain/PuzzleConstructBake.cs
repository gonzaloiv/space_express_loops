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
            PuzzleConstructedField field = PuzzleConstruct.Build(request);
            if (field == null)
                return Fail(PuzzleBakeStatus.NoRun);

            PuzzleBakeResult last = default;
            for (int attempt = 0; attempt < attempts && field != null; attempt++)
            {
                last = Verify(field, request);
                if (last.Baked)
                    return last;

                field = PuzzleConstruct.MoveDecoy(field, request, attempt + 1);
            }

            return last;
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
                    out System.Collections.Generic.List<string> problems, request.Sim))
            {
                Object.DestroyImmediate(result.Puzzle);
                PuzzleBakeResult failed = Fail(PuzzleBakeStatus.ContractFailed, result.Kind);
                failed.Problems = problems;
                return failed;
            }

            float window = PuzzleSolveWindow.Measure(result.Puzzle, result.Solution, request.Sim);
            if (window <= 0f)
            {
                Object.DestroyImmediate(result.Puzzle);
                return Fail(PuzzleBakeStatus.Unverified, result.Kind);
            }

            result.Window = window;
            return result;
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
            puzzle.zoneCentre = (field.Origin + field.Destination) * 0.5f;
            puzzle.cubeSizeMin = field.CubeSizeMin;
            puzzle.cubeSizeMax = field.CubeSizeMax;
            ApplyBrief(puzzle, request.Brief, field.Answer);
            puzzle.entropyBudget = OpenBudget;
            puzzle.captureRadius = puzzle.CaptureRadiusAtDestination();
            return puzzle;
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

        private static PuzzleBakeResult Fail(
            PuzzleBakeStatus status, PuzzleSolutionKind kind = PuzzleSolutionKind.Other) =>
            new() { Status = status, Kind = kind };
    }
}
