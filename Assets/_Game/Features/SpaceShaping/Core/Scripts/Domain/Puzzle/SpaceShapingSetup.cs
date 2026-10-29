using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingSetup
    {
        public Vector3 Origin;
        public Vector3 Destination;
        public int MaxSteps;
        public float LaunchSpeed;
        public float EntropyBudget;
        public PuzzleSimSettings Sim;
        public SourceSpec[] Sources;

        public static SpaceShapingSetup Build(PuzzleData puzzle, PuzzleSimSettings settings)
        {
            PuzzleSimSettings sim = settings;
            float capture = puzzle.captureRadius > 0f
                ? puzzle.captureRadius
                : puzzle.CaptureRadiusAtDestination();
            if (capture > 0f)
                sim.CaptureRadius = capture;

            return new SpaceShapingSetup
            {
                Origin = puzzle.Origin,
                Destination = puzzle.Destination,
                MaxSteps = BudgetSteps(puzzle, settings),
                LaunchSpeed = settings.LaunchSpeed,
                EntropyBudget = puzzle.entropyBudget,
                Sim = sim,
                Sources = SourceSpecs.From(puzzle.sources, settings)
            };
        }

        private static int BudgetSteps(PuzzleData puzzle, PuzzleSimSettings settings)
        {
            if (settings.NominalSpeed <= 0f || settings.StepSecs <= 0f)
                return 1;

            float run = Vector3.Distance(puzzle.Origin, puzzle.Destination);
            return Mathf.Max(1, Mathf.CeilToInt(run / settings.NominalSpeed / settings.StepSecs));
        }
    }
}
