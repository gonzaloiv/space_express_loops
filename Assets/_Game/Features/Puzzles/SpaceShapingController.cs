using System;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingController : MonoBehaviour
    {
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private SpaceShapingView view;

        private PuzzleRun run;
        private readonly PreviewClock clock = new();
        private bool wasScaling;
        private bool frozen;
        private PuzzleData currentPuzzleData;

        public event Action Solved;
        public event Action Failed;

        public event Action PinchCommitted
        {
            add => view.PinchCommitted += value;
            remove => view.PinchCommitted -= value;
        }

        public event Action CubeMoved
        {
            add => view.CubeMoved += value;
            remove => view.CubeMoved -= value;
        }

        public void Enter(PuzzleData puzzleData)
        {
            view.gameObject.SetActive(true);
            currentPuzzleData = puzzleData;
            view.ShipsCompleted += OnFlightCompleted;
            view.ResetRequested += ResetPuzzle;
            view.SetScaleEnvelope(settings.minScaleFactor, settings.maxScaleFactor);
            StartPuzzle();
        }

        public void Exit()
        {
            view.ShipsCompleted -= OnFlightCompleted;
            view.ResetRequested -= ResetPuzzle;
            view.HideBaseline();
            view.SetZonesLocked(false);
            frozen = false;
            run = null;
        }

        public void HideAll()
        {
            view.gameObject.SetActive(false);
        }

        public int Grade => run.Session.Grade();

        public void Rotate(float degrees)
        {
            if (run == null || frozen || Mathf.Approximately(degrees, 0f))
                return;

            FieldFrame.Measure(run.Session, out Vector3 centre, out _);
            Vector3 pivot = view.transform.TransformPoint(centre);
            view.transform.RotateAround(pivot, Vector3.up, degrees);
        }

        public void ResetPuzzle()
        {
            if (frozen)
                return;

            PreparePuzzle();
            run.Reset();
            view.SoftResetShips(run.Session);
            PlayStartCheck();
            view.ShowBaseline(run.Session);
        }

        [ContextMenu("Preview field")]
        public void PreviewField()
        {
            PuzzleRun preview = new(BuildSetup());
            TrajectoryResult[] results = preview.SimulatePreview();
            view.Render(preview.Session);
            view.ShowBaseline(preview.Session);
            view.ShowTrajectories(preview.Session, results, false);
        }

        private void LateUpdate()
        {
            if (run == null || frozen)
                return;

            TrackGesture();
        }

        private void TrackGesture()
        {
            if (view.IsScaling)
            {
                if (!wasScaling && run.Session.IsOverBudget)
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
                clock.Reset();
            }

            run.PreviewZone(view.Zone);
            wasScaling = true;
            RenderPreview();
        }

        private void RenderPreview()
        {
            if (clock.TakeLattice(Time.unscaledTime))
                ApplyAndRender();

            if (clock.TakeReadout(Time.unscaledTime))
                view.ShowReadout(PuzzleReadout.Pending(run.Session));

            PreviewPath();
        }

        private void PreviewPath()
        {
            if (clock.TakePreview(Time.unscaledTime))
                view.ShowTrajectories(run.Session, run.SimulatePreview(), true);
        }

        private void ApplyAndRender()
        {
            view.ApplyFromSession(run.Session);
            view.Render(run.Session);
        }

        private void CommitScale()
        {
            wasScaling = false;
            run.CommitPreview();
            PushField();
            view.HideBaseline();
            view.TravelShips(run.Session, run.LastResults);
        }

        private void ResetView()
        {
            view.ClearPrediction();
            view.SetConditions(currentPuzzleData.ToZoneConditions());
            view.PlaceFrames(currentPuzzleData.frames, settings.zoneFalloff);
        }

        private void StartPuzzle()
        {
            PreparePuzzle();
            run = new PuzzleRun(BuildSetup());
            view.SetPuzzle(currentPuzzleData);
            view.SoftResetShips(run.Session);
            PlayStartCheck();
            view.ShowBaseline(run.Session);
        }

        private void PreparePuzzle()
        {
            wasScaling = false;
            clock.Reset();
            ResetView();
        }

        private void PlayStartCheck()
        {
            PushField(false);
            view.TravelShips(run.Session, run.LastResults);
        }

        private void PushField(bool pendingPath = true)
        {
            view.StopShips();
            TrajectoryResult[] results = run.Simulate();
            ApplyAndRender();
            view.ShowTrajectories(run.Session, results, pendingPath);
            PublishReadout();
        }

        private void OnFlightCompleted()
        {
            if (run == null || frozen || run.LastResults == null)
                return;

            run.Check();
            bool solved = run.Session.IsSolved(run.LastResults);
            view.SetPrediction(solved, run.LastResults.Worst());
            view.ShowTrajectories(run.Session, run.LastResults, false);
            if (!solved)
                view.ShowBaseline(run.Session);
            view.PulseTrajectories();
            PublishReadout();
            if (!solved && !run.Session.IsFailed(run.LastResults))
                return;

            frozen = true;
            view.SetZonesLocked(true);
            if (solved)
                Solved?.Invoke();
            else
                Failed?.Invoke();
        }

        private void PublishReadout()
        {
            if (run.LastResults == null)
                return;

            view.ShowReadout(run.Readout());
        }

        public SpaceShapingSetup BuildSetup() =>
            SpaceShapingSetup.Build(currentPuzzleData, PuzzleSimSettings.From(settings));
    }
}
