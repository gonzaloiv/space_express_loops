using DigitalLove.Game.Spatial;
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
        public bool NoFail;
        public bool NoGrade;
        public PuzzleSimSettings Sim;
        public SourceSpec[] Sources;
        public ShipSpec[] Ships;

        public static SpaceShapingSetup Build(PuzzleData puzzle, PuzzleSimSettings settings) =>
            Build(Inputs(puzzle, settings));

        // The one setup path: the PuzzleData overload above builds the plain inputs for it, and a
        // field snapshot (the bake worker) calls it directly.
        public static SpaceShapingSetup Build(SpaceShapingInputs inputs)
        {
            PuzzleSimSettings sim = inputs.Settings;
            if (inputs.CaptureRadius > 0f)
                sim.CaptureRadius = inputs.CaptureRadius;

            return new SpaceShapingSetup
            {
                Origin = inputs.Origin,
                Destination = inputs.Destination,
                MaxSteps = BudgetSteps(inputs.Ships, inputs.Settings),
                LaunchSpeed = inputs.Settings.LaunchSpeed,
                EntropyBudget = inputs.EntropyBudget,
                NoFail = inputs.NoFail,
                NoGrade = inputs.NoGrade,
                Sim = sim,
                Sources = inputs.Sources,
                Ships = inputs.Ships
            };
        }

        private static SpaceShapingInputs Inputs(PuzzleData puzzle, PuzzleSimSettings settings) => new()
        {
            Origin = puzzle.Origin,
            Destination = puzzle.Destination,
            CaptureRadius = puzzle.EffectiveCaptureRadius,
            EntropyBudget = puzzle.entropyBudget,
            NoFail = puzzle.noFail,
            NoGrade = puzzle.noGrade,
            Settings = settings,
            Sources = SourceSpecs.From(puzzle.sources, settings),
            Ships = ShipSpecs.From(puzzle)
        };

        private static int BudgetSteps(ShipSpec[] ships, PuzzleSimSettings settings)
        {
            float run = 0f;
            for (int i = 0; i < ships.Length; i++)
                run = Mathf.Max(run, Vector3.Distance(ships[i].Origin, ships[i].Destination));

            return PuzzleStepBudget.Steps(run, settings, 1f);
        }
    }
}
