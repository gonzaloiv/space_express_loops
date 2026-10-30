using System;
using DigitalLove.FlowControl;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingController : MonoState
    {
        [SerializeField] private PuzzleData[] puzzles;
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private SpaceShapingView view;

        private readonly TrajectoryIntegrator integrator = new();
        private readonly ShipLaunch launch = new();

        private SpaceShapingSession session;
        private TrajectoryResult[] lastResults;
        private PuzzleReadout readout;
        private int puzzleIndex;
        private bool wasScaling;
        private bool hasResult;
        private bool isChecked;

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
        }

        public void ResetPuzzle()
        {
            wasScaling = false;
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
            view.ShowTrajectories(preview, results);
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
            {
                CommitScale();
                return;
            }

            if (!session.IsOverBudget && view.HasChanged())
                PushField();
        }
        private void BeginScale()
        {
            if (!wasScaling)
            {
                view.StopShips();
                view.ClearPrediction();
            }

            session.PreviewZone(view.Zone);
            wasScaling = true;
            RenderPreview();
        }

        private void RenderPreview()
        {
            ApplyAndRender(session.BuildField());
            readout = PuzzleReadout.Pending(session);
            ReadoutChanged?.Invoke(readout);
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
            PushField();
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
            ResetView();
            session = new SpaceShapingSession(BuildSetup());
            view.SoftResetShips(session);
            PushField();
        }

        private void PushField()
        {
            view.StopShips();
            SpacetimeField field = session.BuildField();
            TrajectoryResult[] results = session.SimulateAll(field, integrator, launch);
            view.SetPrediction(session.IsSolved(results), results.Worst());

            ApplyAndRender(field);
            view.ShowTrajectories(session, results);
            view.Snapshot();

            lastResults = results;
            hasResult = true;
            isChecked = false;
            PublishReadout();
        }

        private void OnFlightCompleted()
        {
            if (session == null || !hasResult)
                return;

            isChecked = true;
            PublishReadout();
            view.PulseTrajectories();
        }

        private void OnRefused(ZoneRefusal reason) => Refused?.Invoke(reason);

        private void PublishReadout()
        {
            if (!hasResult)
                return;

            readout = PuzzleReadout.From(session, lastResults, isChecked);
            ReadoutChanged?.Invoke(readout);
        }

        public SpaceShapingSetup BuildSetup() =>
            SpaceShapingSetup.Build(puzzles[puzzleIndex], PuzzleSimSettings.From(settings));
    }
}
