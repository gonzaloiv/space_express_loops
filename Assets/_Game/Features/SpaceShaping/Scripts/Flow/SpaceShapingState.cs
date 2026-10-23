using DigitalLove.FlowControl;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingState : MonoState
    {
        [SerializeField] private PuzzleData[] puzzles;
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private SpaceShapingView view;
        [SerializeField] private ShapingLabHud hud;
        [SerializeField] private ShapingLabButton resetButton;
        [SerializeField] private ShapingLabButton nextButton;

        private readonly TrajectoryIntegrator integrator = new();
        private readonly ShipLaunch launch = new();

        private SpaceShapingSession session;
        private int puzzleIndex;
        private int editCount;
        private bool wasChanged;

        public override void Enter()
        {
            puzzleIndex = 0;
            StartPuzzle();
        }

        public override void Exit() => session = null;

        private void LateUpdate()
        {
            if (session == null || !parent.IsCurrentState<SpaceShapingState>())
                return;

            if (resetButton.ConsumePressed())
                ResetPuzzle();
            if (nextButton.ConsumePressed())
                NextPuzzle();

            bool changed = view.HasChanged();
            if (changed && !wasChanged)
                editCount++;
            wasChanged = changed;
            if (!changed)
                return;

            view.SyncToSession(session);
            Push();
        }

        private void StartPuzzle()
        {
            editCount = 0;
            wasChanged = false;
            session = new SpaceShapingSession(BuildSetup());
            Push();
        }

        private void ResetPuzzle()
        {
            editCount = 0;
            session.Reset();
            Push();
        }

        private void NextPuzzle()
        {
            puzzleIndex = (puzzleIndex + 1) % puzzles.Length;
            StartPuzzle();
        }

        private void Push()
        {
            session.Constrain();
            view.ApplyFromSession(session);

            SpacetimeField field = session.BuildField();
            TrajectoryResult result = session.Simulate(field, integrator, launch);
            view.Render(field, result, session.Destination);
            view.Snapshot();
            hud.Render(BuildReadout(result));
        }

        private ShapingReadout BuildReadout(TrajectoryResult result)
        {
            PuzzleData puzzle = puzzles[puzzleIndex];
            return new ShapingReadout
            {
                PuzzleName = puzzle.name,
                Outcome = result.Outcome,
                ShipTime = result.ShipTime,
                Deadline = puzzle.deadline,
                Solved = session.IsSolved(result),
                EditCount = editCount
            };
        }

        private SpaceShapingSetup BuildSetup()
        {
            PuzzleData puzzle = puzzles[puzzleIndex];
            return new SpaceShapingSetup
            {
                Origin = puzzle.origin,
                Destination = puzzle.destination,
                CaptureRadius = puzzle.captureRadius,
                StepSecs = settings.stepSecs,
                MaxSteps = settings.maxSteps,
                LaunchModel = settings.launchModel,
                LaunchSpeed = settings.launchSpeed,
                ThrustAcceleration = settings.thrustAcceleration,
                SourceStrengthScale = settings.sourceStrengthScale,
                SeparationScale = settings.separationScale,
                DilationScale = settings.dilationScale,
                Deadline = puzzle.deadline,
                Sources = BuildSpecs(puzzle)
            };
        }

        private static SourceSpec[] BuildSpecs(PuzzleData puzzle)
        {
            SourceSpec[] specs = new SourceSpec[puzzle.sources.Length];
            for (int i = 0; i < specs.Length; i++)
            {
                PuzzleSource source = puzzle.sources[i];
                specs[i] = new SourceSpec
                {
                    Home = source.home,
                    InitialScale = source.scale,
                    MoveRange = source.moveRange,
                    MinScale = source.minScale,
                    MaxScale = source.maxScale
                };
            }

            return specs;
        }
    }
}
