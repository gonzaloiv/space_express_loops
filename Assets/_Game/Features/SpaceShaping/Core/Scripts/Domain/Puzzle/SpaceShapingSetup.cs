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
        public ShipSpec[] Ships;

        public static SpaceShapingSetup Build(PuzzleData puzzle, PuzzleSimSettings settings)
        {
            PuzzleSimSettings sim = settings;
            float capture = puzzle.captureRadius > 0f
                ? puzzle.captureRadius
                : puzzle.CaptureRadiusAtDestination();
            if (capture > 0f)
                sim.CaptureRadius = capture;

            ShipSpec[] ships = ShipSpecs.From(puzzle);

            return new SpaceShapingSetup
            {
                Origin = puzzle.Origin,
                Destination = puzzle.Destination,
                MaxSteps = BudgetSteps(ships, settings),
                LaunchSpeed = settings.LaunchSpeed,
                EntropyBudget = puzzle.entropyBudget,
                Sim = sim,
                Sources = SourceSpecs.From(puzzle.sources, settings),
                Ships = ships
            };
        }

        private static int BudgetSteps(ShipSpec[] ships, PuzzleSimSettings settings)
        {
            if (settings.NominalSpeed <= 0f || settings.StepSecs <= 0f)
                return 1;

            float run = 0f;
            for (int i = 0; i < ships.Length; i++)
                run = Mathf.Max(run, Vector3.Distance(ships[i].Origin, ships[i].Destination));

            return Mathf.Max(1, Mathf.CeilToInt(run / settings.NominalSpeed / settings.StepSecs));
        }
    }
}
