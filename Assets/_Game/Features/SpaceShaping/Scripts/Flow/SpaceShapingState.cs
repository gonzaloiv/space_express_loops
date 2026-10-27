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
            ResetView();
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
            view.Render(preview.BuildField(), preview.Zones, RunRadius(preview.Origin, preview.Destination));
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

        private void ResetView()
        {
            view.ResetZone();
            float size = settings.cubeSize;
            view.SetZoneFrame(
                puzzles[puzzleIndex].zoneCentre, new Vector3(size, size, size), settings.zoneFalloff);
        }

        private void StartPuzzle()
        {
            wasScaling = false;
            ResetView();
            session = new SpaceShapingSession(BuildSetup());
            ship.Reset(session.Origin);
            PushField();
        }

        private TrajectoryResult PushField()
        {
            view.ApplyFromSession(session);

            SpacetimeField field = session.BuildField();
            TrajectoryResult result = session.Simulate(field, integrator, launch);
            view.Render(field, session.Zones, RunRadius(session.Origin, session.Destination));
            view.Snapshot();
            return result;
        }

        private static float RunRadius(Vector3 origin, Vector3 destination) =>
            Vector3.Distance(origin, destination) * 0.5f;

        private SpaceShapingSetup BuildSetup() =>
            SpaceShapingSetup.Build(puzzles[puzzleIndex], PuzzleSimSettings.From(settings));
    }
}
