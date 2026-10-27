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
            view.Render(preview.BuildField(), preview.Zones, FieldRadius(preview));
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
            PuzzleData puzzle = puzzles[puzzleIndex];
            view.SetConditions(ZoneConditions.From(puzzle));
            float size = puzzle.cubeSize > 0f ? puzzle.cubeSize : settings.cubeSize;
            view.SetZoneFrame(puzzle.zoneCentre, new Vector3(size, size, size), settings.zoneFalloff);
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
            view.Render(field, session.Zones, FieldRadius(session));
            view.Snapshot();
            return result;
        }

        private static float FieldRadius(SpaceShapingSession session)
        {
            float outer = 0f;
            for (int i = 0; i < session.SourceCount; i++)
                outer = Mathf.Max(outer, session.GetPosition(i).magnitude + session.GetScale(i) * 0.5f);

            return Mathf.Max(outer, 0.01f);
        }

        private SpaceShapingSetup BuildSetup() =>
            SpaceShapingSetup.Build(puzzles[puzzleIndex], PuzzleSimSettings.From(settings));
    }
}
