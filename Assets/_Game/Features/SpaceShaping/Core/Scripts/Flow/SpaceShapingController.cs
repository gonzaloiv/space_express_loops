using System;
using DigitalLove.FlowControl;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingController : MonoState
    {
        private const float PreviewIntervalSecs = 0.05f;

        [SerializeField] private PuzzleData[] puzzles;
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private SpaceShapingView view;

        private readonly TrajectoryIntegrator integrator = new();
        private readonly ShipLaunch launch = new();

        private SpaceShapingSession session;
        private TrajectoryResult[] lastResults;
        private PuzzleReadout readout;
        private int puzzleIndex;
        private float nextPreviewTime;
        private bool wasScaling;
        private bool hasResult;
        private bool isChecked;
        private bool isFlying;

        public event Action<PuzzleReadout> ReadoutChanged;

        public event Action<ZoneRefusal> Refused;

        public PuzzleReadout Readout => readout;

        public PuzzleData CurrentPuzzle =>
            puzzles != null && puzzleIndex >= 0 && puzzleIndex < puzzles.Length
                ? puzzles[puzzleIndex]
                : null;

        public override void Enter()
        {
            view.ShipsCompleted += OnFlightCompleted;
            view.Refused += OnRefused;
            view.SetScaleEnvelope(settings.minScaleFactor, settings.maxScaleFactor);
            puzzleIndex = 0;
            StartPuzzle();
        }

        public override void Exit()
        {
            view.ShipsCompleted -= OnFlightCompleted;
            view.Refused -= OnRefused;
            session = null;
            isFlying = false;
        }

        public void ResetPuzzle()
        {
            wasScaling = false;
            isFlying = false;
            ResetView();
            session.Reset();
            view.SoftResetShips(session);
            PushField();
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
            view.Render(field, preview);
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
            }

            session.PreviewZone(view.Zone);
            wasScaling = true;
            RenderPreview();
        }

        private void RenderPreview()
        {
            SpacetimeField field = session.BuildField();
            ApplyAndRender(field);
            readout = PuzzleReadout.Pending(session);
            ReadoutChanged?.Invoke(readout);
            PreviewPath(field);
        }

        private void PreviewPath(SpacetimeField field)
        {
            if (Time.unscaledTime < nextPreviewTime)
                return;

            nextPreviewTime = Time.unscaledTime + PreviewIntervalSecs;
            lastResults = session.SimulateAll(field, integrator, launch);
            hasResult = true;
            view.ShowTrajectories(session, lastResults, true);
        }

        private void ApplyAndRender(SpacetimeField field)
        {
            view.ApplyFromSession(session);
            view.Render(field, session);
        }

        private void CommitScale()
        {
            wasScaling = false;
            session.CommitPreview();
            PushField(true);
            view.TravelShips(session, lastResults);
        }

        private void ResetView()
        {
            PuzzleData puzzle = puzzles[puzzleIndex];
            view.SetConditions(puzzle.ToZoneConditions());
            view.PlaceFrames(puzzle.frames, settings.zoneFalloff);
        }

        private void StartPuzzle()
        {
            wasScaling = false;
            isChecked = false;
            isFlying = false;
            nextPreviewTime = 0f;
            ResetView();
            session = new SpaceShapingSession(BuildSetup());
            view.SoftResetShips(session);
            PushField();
        }

        private void PushField(bool startFlight = false)
        {
            isFlying = startFlight;
            view.StopShips();
            SpacetimeField field = session.BuildField();
            TrajectoryResult[] results = session.SimulateAll(field, integrator, launch);

            ApplyAndRender(field);
            view.ShowTrajectories(session, results, true);

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
            view.SetPrediction(session.IsSolved(lastResults), lastResults.Worst());
            view.ShowTrajectories(session, lastResults, false);
            view.PulseTrajectories();
            PublishReadout();
        }

        private void OnRefused(ZoneRefusal reason) => Refused?.Invoke(reason);

        private void PublishReadout()
        {
            if (!hasResult)
                return;

            readout = PuzzleReadout.From(session, lastResults, isChecked, isFlying);
            ReadoutChanged?.Invoke(readout);
        }

        public SpaceShapingSetup BuildSetup() =>
            SpaceShapingSetup.Build(puzzles[puzzleIndex], PuzzleSimSettings.From(settings));
    }
}
