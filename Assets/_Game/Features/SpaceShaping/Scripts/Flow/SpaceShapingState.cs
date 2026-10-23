using DigitalLove.FlowControl;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingState : MonoState
    {
        [SerializeField] private PuzzleData puzzle;
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private SpaceShapingView view;

        private readonly TrajectoryIntegrator integrator = new();
        private readonly ShipLaunch launch = new();

        private SpaceShapingSession session;

        public override void Enter()
        {
            session = new SpaceShapingSession(BuildSetup());
            Apply();
        }

        public override void Exit() => session = null;

        private void Update()
        {
            if (session == null || !parent.IsCurrentState<SpaceShapingState>() || !view.HasChanged())
                return;

            Apply();
        }

        private void Apply()
        {
            view.SyncToSession(session);
            session.Constrain();
            view.ApplyFromSession(session);

            SpacetimeField field = session.BuildField();
            TrajectoryResult result = session.Simulate(field, integrator, launch);
            view.Render(field, result, session.Destination);
            view.Snapshot();
        }

        private SpaceShapingSetup BuildSetup()
        {
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
                Sources = BuildSpecs()
            };
        }

        private SourceSpec[] BuildSpecs()
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
