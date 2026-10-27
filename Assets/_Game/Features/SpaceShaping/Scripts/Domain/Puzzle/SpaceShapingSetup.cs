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
            MaxSteps = settings.MaxSteps,
            LaunchSpeed = puzzle.launchSpeed,
            SourceStrengthScale = settings.SourceStrengthScale,
            SeparationScale = settings.SeparationScale,
            DilationScale = settings.DilationScale,
            Tmin = puzzle.tmin,
            Tmax = puzzle.tmax,
            EntropyBudget = puzzle.entropyBudget,
            Sources = SourceSpecs.From(puzzle.sources, settings)
        };
    }
}
