using System;
using DigitalLove.FlowControl;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingState : MonoState
    {
        [SerializeField] private PuzzleData[] puzzles;
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private SpaceShapingView view;
        [SerializeField] private ShipTravelView ship;
        [SerializeField] private float clockPublishSecs = 0.1f;

        private readonly TrajectoryIntegrator integrator = new();
        private readonly ShipLaunch launch = new();

        private SpaceShapingSession session;
        private TrajectoryResult lastResult;
        private PuzzleReadout readout;
        private int puzzleIndex;
        private bool wasScaling;
        private bool hasResult;
        private bool isChecked;
        private float nextClockPublish;

        public event Action<PuzzleReadout> ReadoutChanged;

        public PuzzleReadout Readout => readout;

        public SpaceZone Zone => view.Zone;

        public PuzzleData CurrentPuzzle =>
            puzzles != null && puzzleIndex >= 0 && puzzleIndex < puzzles.Length
                ? puzzles[puzzleIndex]
                : null;

        public override void Enter()
        {
            ship.Completed += OnFlightCompleted;
            view.SetScaleEnvelope(settings.minScaleFactor, settings.maxScaleFactor);
            puzzleIndex = 0;
            StartPuzzle();
        }

        public override void Exit()
        {
            ship.Completed -= OnFlightCompleted;
            session = null;
        }

        public void ResetPuzzle()
        {
            wasScaling = false;
            ResetView();
            session.Reset();
            ship.SoftReset(session.Origin);
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
            view.Render(preview.BuildField(), preview);
        }

        public bool ApplySolvedScale(float scale)
        {
            if (session == null || scale <= 0f)
                return false;

            ResetPuzzle();
            view.SetZoneScale(scale);
            session.ApplyZone(view.Zone);
            ship.Travel(PushField());
            return true;
        }

        private void LateUpdate()
        {
            if (session == null || !parent.IsCurrentState<SpaceShapingState>())
                return;

            TrackGesture();
            TrackFlight();
        }

        private void TrackGesture()
        {
            if (view.IsScaling)
            {
                if (!wasScaling && session.IsOverBudget)
                {
                    view.Refuse();
                    return;
                }

                BeginScale();
                return;
            }

            if (wasScaling)
            {
                CommitScale();
                return;
            }

            if (!session.IsOverBudget && view.HasChanged())
                PushField();
        }
        private void BeginScale()
        {
            session.PreviewZone(view.Zone);
            wasScaling = true;
            PushField();
        }

        private void TrackFlight()
        {
            if (!ship.IsFlying || !hasResult || Time.unscaledTime < nextClockPublish)
                return;

            nextClockPublish = Time.unscaledTime + Mathf.Max(clockPublishSecs, 0.01f);
            readout = PuzzleReadout.From(session, lastResult, false, ship.Clock);
            ReadoutChanged?.Invoke(readout);
        }

        private void CommitScale()
        {
            wasScaling = false;
            session.CommitPreview();
            ship.Travel(PushField());
        }

        private void ResetView()
        {
            PuzzleData puzzle = puzzles[puzzleIndex];
            view.ResetZone();
            view.SetConditions(puzzle.ToZoneConditions());
            float size = puzzle.cubeSize;
            view.SetZoneFrame(puzzle.zoneCentre, new Vector3(size, size, size), settings.zoneFalloff);
        }

        private void StartPuzzle()
        {
            wasScaling = false;
            isChecked = false;
            ResetView();
            session = new SpaceShapingSession(BuildSetup());
            ship.SoftReset(session.Origin);
            PushField();
        }

        private TrajectoryResult PushField()
        {
            ship.Stop();
            view.ApplyFromSession(session);

            SpacetimeField field = session.BuildField();
            TrajectoryResult result = session.Simulate(field, integrator, launch);
            bool solved = session.IsSolved(result);
            view.SetPrediction(solved, result.Outcome);

            view.Render(field, session);
            view.ShowTrajectory(result, solved);
            view.Snapshot();

            lastResult = result;
            hasResult = true;
            isChecked = false;
            nextClockPublish = 0f;
            PublishReadout();
            return result;
        }

        private void OnFlightCompleted()
        {
            if (session == null || !hasResult)
                return;

            isChecked = true;
            PublishReadout();
            view.PulseTrajectory();
        }

        private void PublishReadout()
        {
            if (!hasResult)
                return;

            readout = PuzzleReadout.From(session, lastResult, isChecked);
            ReadoutChanged?.Invoke(readout);
        }

        public SpaceShapingSetup BuildSetup() =>
            SpaceShapingSetup.Build(puzzles[puzzleIndex], PuzzleSimSettings.From(settings));
    }
}
