using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingSetup
    {
        public Vector3 Origin;
        public Vector3 Destination;
        public int MaxSteps;
        public float LaunchSpeed;
        public float Tmin;
        public float Tmax;
        public float EntropyBudget;
        public PuzzleSimSettings Sim;
        public SourceSpec[] Sources;
        public int AnchorSource = -1;

        public static SpaceShapingSetup Build(PuzzleData puzzle, PuzzleSimSettings settings)
        {
            PuzzleSimSettings sim = settings;
            if (puzzle.captureRadius > 0f)
                sim.CaptureRadius = puzzle.captureRadius;

            SourceSpec[] sources = SourceSpecs.From(puzzle.sources, settings);
            return new SpaceShapingSetup
            {
                Origin = puzzle.Origin,
                Destination = puzzle.Destination,
                MaxSteps = BudgetSteps(puzzle, settings),
                LaunchSpeed = puzzle.launchSpeed,
                Tmin = puzzle.tmin,
                Tmax = puzzle.tmax,
                EntropyBudget = puzzle.entropyBudget,
                Sim = sim,
                Sources = sources,
                AnchorSource = Anchor(sources, puzzle.Origin)
            };
        }

        private static int Anchor(SourceSpec[] sources, Vector3 origin)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                if ((sources[i].Home - origin).sqrMagnitude < 0.000001f)
                    return i;
            }

            return -1;
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
