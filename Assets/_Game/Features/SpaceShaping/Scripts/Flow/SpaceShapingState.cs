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
        private TrajectoryResult launched;
        private bool wasScaling;
        private bool flying;

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
            TrackFlight();
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

        private void TrackFlight()
        {
            bool isFlying = ship.IsFlying;
            if (flying && !isFlying && !view.IsScaling)
                hud.Render(BuildReadout(launched, true));

            flying = isFlying;
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
            launched = PushField();
            ship.Travel(launched);
        }

        private void StartPuzzle()
        {
            wasScaling = false;
            flying = false;
            view.ResetZone();
            session = new SpaceShapingSession(BuildSetup());
            ship.Reset(session.Origin);
            hud.Render(BuildReadout(PushField(), false));
        }

        private void ResetPuzzle()
        {
            wasScaling = false;
            flying = false;
            view.ResetZone();
            session.Reset();
            ship.Reset(session.Origin);
            hud.Render(BuildReadout(PushField(), false));
        }

        private void NextPuzzle()
        {
            puzzleIndex = (puzzleIndex + 1) % puzzles.Length;
            StartPuzzle();
        }

        private TrajectoryResult PushField()
        {
            view.ApplyFromSession(session);

            SpacetimeField field = session.BuildField();
            TrajectoryResult result = session.Simulate(field, integrator, launch);
            view.Render(session.Zones);
            view.Snapshot();
            return result;
        }

        private ShapingReadout BuildReadout(TrajectoryResult result, bool verified)
        {
            PuzzleData puzzle = puzzles[puzzleIndex];
            return new ShapingReadout
            {
                PuzzleName = puzzle.name,
                Verified = verified,
                Outcome = result.Outcome,
                ShipTime = result.ShipTime,
                Tmin = puzzle.tmin,
                Tmax = puzzle.tmax,
                EntropyLeft = puzzle.entropyBudget - session.EntropySpent,
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
