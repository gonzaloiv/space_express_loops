using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingSetup
    {
        public Vector3 Origin;
        public Vector3 Destination;
        public float CaptureRadius;
        public float StepSecs;
        public int MaxSteps;
        public float LaunchSpeed;
        public float SourceStrengthScale;
        public float SeparationScale;
        public float DilationScale;
        public float Tmin;
        public float Tmax;
        public float EntropyBudget;
        public SourceSpec[] Sources;

        public static SpaceShapingSetup Build(PuzzleData puzzle, PuzzleSimSettings settings) => new()
        {
            Origin = puzzle.Origin,
            Destination = puzzle.Destination,
            CaptureRadius = settings.CaptureRadius,
            StepSecs = settings.StepSecs,
            MaxSteps = BudgetSteps(puzzle, settings),
            LaunchSpeed = puzzle.launchSpeed,
            SourceStrengthScale = settings.SourceStrengthScale,
            SeparationScale = settings.SeparationScale,
            DilationScale = settings.DilationScale,
            Tmin = puzzle.tmin,
            Tmax = puzzle.tmax,
            EntropyBudget = puzzle.entropyBudget,
            Sources = SourceSpecs.From(puzzle.sources, settings)
        };

        private static int BudgetSteps(PuzzleData puzzle, PuzzleSimSettings settings)
        {
            if (settings.NominalSpeed <= 0f || settings.StepSecs <= 0f)
                return 1;

            float run = Vector3.Distance(puzzle.Origin, puzzle.Destination);
            return Mathf.Max(1, Mathf.CeilToInt(run / settings.NominalSpeed / settings.StepSecs));
        }
    }
}
