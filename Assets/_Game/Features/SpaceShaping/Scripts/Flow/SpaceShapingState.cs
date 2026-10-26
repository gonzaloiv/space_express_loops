using DigitalLove.FlowControl;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingState : MonoState
    {
        [SerializeField] private PuzzleData[] puzzles;
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private SpaceShapingView view;
        [SerializeField] private ShipTravelView ship;

        private readonly TrajectoryIntegrator integrator = new();
        private readonly ShipLaunch launch = new();

        private SpaceShapingSession session;
        private int puzzleIndex;
        private bool wasScaling;

        public override void Enter()
        {
            puzzleIndex = 0;
            StartPuzzle();
        }

        public override void Exit() => session = null;

        public void ResetPuzzle()
        {
            wasScaling = false;
            view.ResetZone();
            session.Reset();
            ship.Reset(session.Origin);
            PushField();
        }

        public void NextPuzzle()
        {
            puzzleIndex = (puzzleIndex + 1) % puzzles.Length;
            StartPuzzle();
        }

        public void PreviewField()
        {
            SpaceShapingSession preview = new(BuildSetup());
            view.SetEndpoints(preview.Origin, preview.Destination);
            view.Render(preview.BuildField(), preview.Zones);
        }

        private void LateUpdate()
        {
            if (session == null || !parent.IsCurrentState<SpaceShapingState>())
                return;

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
                PushField();
        }

        private void BeginScale()
        {
            session.PreviewZone(view.Zone);
            wasScaling = true;
            PushField();
        }

        private void CommitScale()
        {
            wasScaling = false;
            session.CommitPreview();
            ship.Travel(PushField());
        }

        private void StartPuzzle()
        {
            wasScaling = false;
            view.ResetZone();
            session = new SpaceShapingSession(BuildSetup());
            ship.Reset(session.Origin);
            PushField();
        }

        private TrajectoryResult PushField()
        {
            view.ApplyFromSession(session);

            SpacetimeField field = session.BuildField();
            TrajectoryResult result = session.Simulate(field, integrator, launch);
            view.Render(field, session.Zones);
            view.Snapshot();
            return result;
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
