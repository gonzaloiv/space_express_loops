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
        [SerializeField] private ShipTravelView ship;

        private readonly TrajectoryIntegrator integrator = new();
        private readonly ShipLaunch launch = new();

        private SpaceShapingSession session;
        private TrajectoryResult lastResult;
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
            ship.Completed += OnFlightCompleted;
            view.Refused += OnRefused;
            view.SetScaleEnvelope(settings.minScaleFactor, settings.maxScaleFactor);
            puzzleIndex = 0;
            StartPuzzle();
        }

        public override void Exit()
        {
            ship.Completed -= OnFlightCompleted;
            view.Refused -= OnRefused;
            session = null;
        }

        public void ResetPuzzle()
        {
            wasScaling = false;
            ResetView();
            session.Reset();
            ship.SoftReset(view.LaunchPoint(session));
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
            TrajectoryResult result = preview.Simulate(field, integrator, launch);
            view.Render(field, preview);
            view.ShowTrajectory(view.ClipToLaunch(result, preview), preview.IsSolved(result));
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
                ship.Stop();
                view.ClearPrediction();
            }

            session.PreviewZone(view.Zone);
            wasScaling = true;
            RenderPreview();
        }

        private void RenderPreview()
        {
            view.ApplyFromSession(session);
            view.Render(session.BuildField(), session);
            readout = PuzzleReadout.Pending(session);
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
            view.SetConditions(puzzle.ToZoneConditions());
            view.PlaceFrames(puzzle.frames, settings.zoneFalloff);
        }

        private void StartPuzzle()
        {
            wasScaling = false;
            isChecked = false;
            ResetView();
            session = new SpaceShapingSession(BuildSetup());
            ship.SoftReset(view.LaunchPoint(session));
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
            TrajectoryResult visible = view.ClipToLaunch(result, session);
            view.ShowTrajectory(visible, solved);
            view.Snapshot();

            lastResult = result;
            hasResult = true;
            isChecked = false;
            PublishReadout();
            return visible;
        }

        private void OnFlightCompleted()
        {
            if (session == null || !hasResult)
                return;

            isChecked = true;
            PublishReadout();
            view.PulseTrajectory();
        }

        private void OnRefused(ZoneRefusal reason) => Refused?.Invoke(reason);

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
