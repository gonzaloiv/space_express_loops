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
        [SerializeField] private ShipTravelView ship;

        private readonly TrajectoryIntegrator integrator = new();
        private readonly ShipLaunch launch = new();

        private SpaceShapingSession session;
        private int puzzleIndex;
        private TrajectoryResult? lastResult;
        private bool wasScaling;

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
            {
                ResetPuzzle();
                return;
            }

            if (nextButton.ConsumePressed())
            {
                NextPuzzle();
                return;
            }

            TrackGesture();
        }

        private void TrackGesture()
        {
            if (view.IsScaling)
            {
                BeginScale();
                return;
            }

            if (wasScaling)
            {
                CommitScale();
                return;
            }

            if (view.HasChanged())
                Push();
        }

        private void BeginScale()
        {
            session.PreviewZone(view.Zone);
            wasScaling = true;
            Push();
        }

        private void CommitScale()
        {
            wasScaling = false;
            session.CommitPreview();
            Push();
            ship.Travel(lastResult.Value);
        }

        private void StartPuzzle()
        {
            wasScaling = false;
            view.ResetZone();
            session = new SpaceShapingSession(BuildSetup());
            ship.Reset(session.Origin);
            Push();
        }

        private void ResetPuzzle()
        {
            wasScaling = false;
            view.ResetZone();
            session.Reset();
            ship.Reset(session.Origin);
            Push();
        }

        private void NextPuzzle()
        {
            puzzleIndex = (puzzleIndex + 1) % puzzles.Length;
            StartPuzzle();
        }

        private void Push()
        {
            view.ApplyFromSession(session);

            SpacetimeField field = session.BuildField();
            TrajectoryResult result = session.Simulate(field, integrator, launch);
            lastResult = result;
            view.Render(field, session.Zones);
            view.Snapshot();
            hud.Render(BuildReadout(result));
        }

        private ShapingReadout BuildReadout(TrajectoryResult result)
        {
            PuzzleData puzzle = puzzles[puzzleIndex];
            float entropyLeft = puzzle.entropyBudget - session.EntropySpent;
            return new ShapingReadout
            {
                PuzzleName = puzzle.name,
                Outcome = result.Outcome,
                ShipTime = result.ShipTime,
                Tmin = puzzle.tmin,
                Tmax = puzzle.tmax,
                EntropyLeft = entropyLeft,
                Stars = session.Grade(),
                Scale = view.Zone.Scale,
                Solved = session.IsSolved(result),
                Zones = session.CommandCount
            };
        }

        private SpaceShapingSetup BuildSetup()
        {
            PuzzleData puzzle = puzzles[puzzleIndex];
            return new SpaceShapingSetup
            {
                Origin = puzzle.Origin,
                Destination = puzzle.Destination,
                CaptureRadius = settings.captureRadius,
                StepSecs = settings.stepSecs,
                MaxSteps = settings.maxSteps,
                LaunchSpeed = settings.launchSpeed,
                SourceStrengthScale = settings.sourceStrengthScale,
                SeparationScale = settings.separationScale,
                DilationScale = settings.dilationScale,
                Tmin = puzzle.tmin,
                Tmax = puzzle.tmax,
                EntropyBudget = puzzle.entropyBudget,
                Sources = SourceSpecs.From(puzzle.sources, settings)
            };
        }
    }
}
