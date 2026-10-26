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
        [SerializeField] private ShapingLabButton predictButton;
        [SerializeField] private ShipTravelView ship;
        [SerializeField] private float travelIdleSecs = 0.5f;

        private readonly TrajectoryIntegrator integrator = new();
        private readonly ShipLaunch launch = new();

        private SpaceShapingSession session;
        private int puzzleIndex;
        private bool predictionEnabled = true;
        private TrajectoryResult? lastResult;
        private bool travelled;
        private bool wasScaling;
        private float idleElapsed;

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

            if (predictButton.ConsumePressed())
                TogglePrediction();

            bool scaling = view.IsScaling;
            if (scaling)
                session.PreviewZone(view.Zone);
            else if (wasScaling)
                session.CommitPreview();

            bool changed = view.HasChanged();
            if (scaling || changed || wasScaling)
            {
                travelled = false;
                idleElapsed = 0f;
                wasScaling = scaling;
                Push();
                return;
            }

            if (travelled || !lastResult.HasValue || !session.IsSolved(lastResult.Value))
                return;

            idleElapsed += Time.deltaTime;
            if (idleElapsed >= travelIdleSecs)
            {
                travelled = true;
                ship.Travel(lastResult.Value);
            }
        }

        private void StartPuzzle()
        {
            travelled = false;
            wasScaling = false;
            idleElapsed = 0f;
            view.ResetZone();
            session = new SpaceShapingSession(BuildSetup());
            ship.Reset(session.Origin);
            Push();
        }

        private void ResetPuzzle()
        {
            travelled = false;
            wasScaling = false;
            idleElapsed = 0f;
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

        private void TogglePrediction()
        {
            predictionEnabled = !predictionEnabled;
            view.SetPredictionVisible(predictionEnabled);
        }

        private void Push()
        {
            view.ApplyFromSession(session);

            SpacetimeField field = session.BuildField();
            TrajectoryResult result = session.Simulate(field, integrator, launch);
            lastResult = result;
            view.Render(field, result, session.Destination, session.Zones);
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
                Stars = session.Grade(entropyLeft),
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
                Tmin = puzzle.tmin,
                Tmax = puzzle.tmax,
                EntropyBudget = puzzle.entropyBudget,
                EntropyPar = puzzle.entropyPar,
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
