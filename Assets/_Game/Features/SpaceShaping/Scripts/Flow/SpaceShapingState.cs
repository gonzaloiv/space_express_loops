using System;
using DigitalLove.FlowControl;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingState : MonoState
    {
        [SerializeField] private PuzzleData puzzle;
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private SpaceShapingView view;
        [SerializeField] private ShipTravelView ship;
        [SerializeField] private float clockPublishSecs = 0.1f;

        private readonly TrajectoryIntegrator integrator = new();
        private readonly ShipLaunch launch = new();

        private SpaceShapingSession session;
        private TrajectoryResult lastResult;
        private PuzzleReadout readout;
        private bool wasScaling;
        private bool hasResult;
        private bool isChecked;
        private float nextClockPublish;
        private int resultRevision;
        private int pendingResultRevision;

        public event Action<PuzzleReadout> ReadoutChanged;
        public event Action<PuzzleResult> ResultChanged;

        public PuzzleReadout Readout => readout;

        public PuzzleResult Result { get; private set; }

        public SpaceZone Zone => view.Zone;

        public PuzzleData CurrentPuzzle => puzzle;

        public override void Enter()
        {
            ship.Completed += OnFlightCompleted;
            StartPuzzle();
        }

        public override void Exit()
        {
            ship.Completed -= OnFlightCompleted;
            ship.Stop();
            InvalidateResult();
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

        internal void NextPuzzle() => StartPuzzle();

        public void PreviewField()
        {
            SpaceShapingSession preview = new(BuildSetup());
            view.Render(
                preview.BuildField(), preview.Zones, preview.Origin, preview.Destination,
                view.CrossHalf(preview, puzzle, settings.cubeSize));
        }

        public bool ApplySolvedScale(float scale)
        {
            if (session == null || scale <= 0f)
                return false;

            ResetPuzzle();
            view.SetZoneScale(scale);
            session.ApplyZone(view.Zone);
            Launch(PushField());
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
            Launch(PushField());
        }

        private void ResetView()
        {
            view.ResetZone();
            view.SetConditions(ZoneConditions.From(puzzle));
            float size = puzzle.cubeSize > 0f ? puzzle.cubeSize : settings.cubeSize;
            view.SetZoneFrame(puzzle.zoneCentre, new Vector3(size, size, size), settings.zoneFalloff);
        }

        private void StartPuzzle()
        {
            wasScaling = false;
            isChecked = false;
            InvalidateResult();
            ResetView();
            session = new SpaceShapingSession(BuildSetup());
            ship.SoftReset(session.Origin);
            PushField();
        }

        private TrajectoryResult PushField()
        {
            InvalidateResult();
            ship.Stop();
            view.ApplyFromSession(session);

            SpacetimeField field = session.BuildField();
            TrajectoryResult result = session.Simulate(field, integrator, launch);
            bool solved = session.IsSolved(result);
            view.SetPrediction(solved, result.Outcome);

            view.Render(
                field, session.Zones, session.Origin, session.Destination,
                view.CrossHalf(session, puzzle, settings.cubeSize));
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
            if (session == null || pendingResultRevision == 0
                || pendingResultRevision != resultRevision)
                return;

            PublishResult();
            view.PulseTrajectory();
        }

        private void Launch(TrajectoryResult result)
        {
            pendingResultRevision = resultRevision;
            ship.Travel(result);
            if (!ship.IsFlying)
                PublishResult();
        }

        private void PublishResult()
        {
            pendingResultRevision = 0;
            isChecked = true;
            PuzzleResult result = PuzzleResult.From(session, lastResult);
            Result = result;
            PublishReadout();
            ResultChanged?.Invoke(result);
            Debug.Log($"[SpaceShaping] {result.Outcome}; grade {result.Grade}.");
        }

        private void InvalidateResult()
        {
            resultRevision++;
            pendingResultRevision = 0;
            if (Result == null)
                return;

            Result = null;
            ResultChanged?.Invoke(null);
        }

        private void PublishReadout()
        {
            if (!hasResult)
                return;

            readout = PuzzleReadout.From(session, lastResult, isChecked);
            ReadoutChanged?.Invoke(readout);
        }

        public SpaceShapingSetup BuildSetup() =>
            SpaceShapingSetup.Build(puzzle, PuzzleSimSettings.From(settings));
    }
}
