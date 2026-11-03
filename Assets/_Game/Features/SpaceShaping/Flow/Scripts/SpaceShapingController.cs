using System.Collections.Generic;
using DigitalLove.FlowControl;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingController : MonoState
    {
        private const float PreviewIntervalSecs = 0.075f;
        private const float LatticeIntervalSecs = 1f / 30f;
        private const float ReadoutIntervalSecs = 0.1f;

        [SerializeField] private PuzzleData[] puzzles;
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private SpaceShapingView view;

        private readonly TrajectoryIntegrator integrator = new();
        private readonly ShipLaunch launch = new();

        private SpaceShapingSession session;
        private TrajectoryResult[] lastResults;
        private List<Vector3>[] trajectoryBuffers;
        private int puzzleIndex;
        private float nextPreviewTime;
        private float nextLatticeTime;
        private float nextReadoutTime;
        private bool wasScaling;
        private bool hasResult;
        private bool isChecked;
        private bool isFlying;

        public PuzzleData CurrentPuzzle =>
            puzzles != null && puzzleIndex >= 0 && puzzleIndex < puzzles.Length
                ? puzzles[puzzleIndex]
                : null;

        public override void Enter()
        {
            view.ShipsCompleted += OnFlightCompleted;
            view.ResetRequested += ResetPuzzle;
            view.NextRequested += NextPuzzle;
            view.SetScaleEnvelope(settings.minScaleFactor, settings.maxScaleFactor);
            puzzleIndex = 0;
            StartPuzzle();
        }

        public override void Exit()
        {
            view.ShipsCompleted -= OnFlightCompleted;
            view.ResetRequested -= ResetPuzzle;
            view.NextRequested -= NextPuzzle;
            view.HideBaseline();
            session = null;
            isFlying = false;
        }

        public void ResetPuzzle()
        {
            wasScaling = false;
            isFlying = false;
            nextPreviewTime = 0f;
            nextLatticeTime = 0f;
            nextReadoutTime = 0f;
            ResetView();
            session.Reset();
            view.SoftResetShips(session);
            PlayStartCheck();
            view.ShowBaseline(session);
        }

        public void NextPuzzle()
        {
            puzzleIndex = (puzzleIndex + 1) % puzzles.Length;
            StartPuzzle();
        }

        [ContextMenu("Preview field")]
        public void PreviewField()
        {
            SpaceShapingSession preview = new(BuildSetup());
            SpacetimeField field = preview.BuildField();
            TrajectoryResult[] results = preview.SimulateAll(field, integrator, launch);
            view.Render(preview);
            view.ShowBaseline(preview);
            view.ShowTrajectories(preview, results, false);
        }

        private void LateUpdate()
        {
            if (session == null || !parent.IsCurrentState<SpaceShapingController>())
                return;

            TrackGesture();
        }

        private void TrackGesture()
        {
            if (view.IsScaling)
            {
                if (!wasScaling && session.IsOverBudget)
                {
                    view.Refuse(ZoneRefusal.OutOfEntropy);
                    return;
                }

                BeginScale();
                return;
            }

            if (wasScaling)
                CommitScale();
        }

        private void BeginScale()
        {
            if (!wasScaling)
            {
                view.StopShips();
                view.ClearPrediction();
                isFlying = false;
                nextPreviewTime = 0f;
                nextLatticeTime = 0f;
                nextReadoutTime = 0f;
            }

            session.PreviewZone(view.Zone);
            wasScaling = true;
            RenderPreview();
        }

        private void RenderPreview()
        {
            if (Time.unscaledTime >= nextLatticeTime)
            {
                nextLatticeTime = Time.unscaledTime + LatticeIntervalSecs;
                ApplyAndRender();
            }

            if (Time.unscaledTime >= nextReadoutTime)
            {
                nextReadoutTime = Time.unscaledTime + ReadoutIntervalSecs;
                view.ShowReadout(PuzzleReadout.Pending(session));
            }

            PreviewPath();
        }

        private void PreviewPath()
        {
            if (Time.unscaledTime < nextPreviewTime)
                return;

            nextPreviewTime = Time.unscaledTime + PreviewIntervalSecs;
            SpacetimeField field = session.BuildField();
            lastResults = session.SimulateAll(field, integrator, launch, trajectoryBuffers);
            hasResult = true;
            view.ShowTrajectories(session, lastResults, true);
        }

        private void ApplyAndRender()
        {
            view.ApplyFromSession(session);
            view.Render(session);
        }

        private void CommitScale()
        {
            wasScaling = false;
            session.CommitPreview();
            PushField(true);
            view.HideBaseline();
            view.TravelShips(session, lastResults);
        }

        private void ResetView()
        {
            PuzzleData puzzle = puzzles[puzzleIndex];
            view.ClearPrediction();
            view.SetConditions(puzzle.ToZoneConditions());
            view.PlaceFrames(puzzle.frames, settings.zoneFalloff);
        }

        private void StartPuzzle()
        {
            wasScaling = false;
            isChecked = false;
            isFlying = false;
            nextPreviewTime = 0f;
            nextLatticeTime = 0f;
            nextReadoutTime = 0f;
            ResetView();
            session = new SpaceShapingSession(BuildSetup());
            EnsureTrajectoryBuffers();
            view.SetPuzzle(CurrentPuzzle);
            view.SoftResetShips(session);
            PlayStartCheck();
            view.ShowBaseline(session);
        }

        private void PlayStartCheck()
        {
            PushField(true, false);
            view.ShowTrajectories(session, lastResults, false);
            view.TravelShips(session, lastResults);
        }

        private void PushField(bool startFlight = false, bool showPath = true)
        {
            isFlying = startFlight;
            view.StopShips();
            SpacetimeField field = session.BuildField();
            TrajectoryResult[] results = session.SimulateAll(
                field, integrator, launch, trajectoryBuffers);

            ApplyAndRender();
            if (showPath)
                view.ShowTrajectories(session, results, true);
            else
                view.HideTrajectories();

            lastResults = results;
            hasResult = true;
            isChecked = false;
            PublishReadout();
        }

        private void OnFlightCompleted()
        {
            if (session == null || !hasResult || lastResults == null)
                return;

            isChecked = true;
            isFlying = false;
            bool solved = session.IsSolved(lastResults);
            view.SetPrediction(solved, lastResults.Worst());
            view.ShowTrajectories(session, lastResults, false);
            if (!solved)
                view.ShowBaseline(session);
            view.PulseTrajectories();
            PublishReadout();
        }

        private void PublishReadout()
        {
            if (!hasResult)
                return;

            view.ShowReadout(PuzzleReadout.From(session, lastResults, isChecked, isFlying));
        }

        private void EnsureTrajectoryBuffers()
        {
            if (trajectoryBuffers != null && trajectoryBuffers.Length == session.ShipCount)
                return;

            trajectoryBuffers = new List<Vector3>[session.ShipCount];
            for (int i = 0; i < trajectoryBuffers.Length; i++)
                trajectoryBuffers[i] = new List<Vector3>(session.MaxSteps + 1);
        }

        public SpaceShapingSetup BuildSetup() =>
            SpaceShapingSetup.Build(puzzles[puzzleIndex], PuzzleSimSettings.From(settings));
    }
}
