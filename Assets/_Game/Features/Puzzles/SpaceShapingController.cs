using System;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingController : MonoBehaviour
    {
        private const float PreviewIntervalSecs = 0.075f;
        private const float LatticeIntervalSecs = 1f / 30f;
        private const float ReadoutIntervalSecs = 0.1f;

        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private SpaceShapingView view;

        private PuzzleRun run;
        private float nextPreviewTime;
        private float nextLatticeTime;
        private float nextReadoutTime;
        private bool wasScaling;
        private bool solvedFired;
        private PuzzleData currentPuzzleData;

        public event Action Solved;

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
            run = null;
        }

        public void HideAll()
        {
            view.gameObject.SetActive(false);
        }

        public void Rotate(float degrees)
        {
            if (run == null || Mathf.Approximately(degrees, 0f))
                return;

            FieldFrame.Measure(run.Session, out Vector3 centre, out _);
            Vector3 pivot = view.transform.TransformPoint(centre);
            view.transform.RotateAround(pivot, Vector3.up, degrees);
        }

        public void ResetPuzzle()
        {
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
            if (run == null)
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
                ClearTimers();
            }

            run.PreviewZone(view.Zone);
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
                view.ShowReadout(PuzzleReadout.Pending(run.Session));
            }

            PreviewPath();
        }

        private void PreviewPath()
        {
            if (Time.unscaledTime < nextPreviewTime)
                return;

            nextPreviewTime = Time.unscaledTime + PreviewIntervalSecs;
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
            solvedFired = false;
            ClearTimers();
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
            if (run == null || run.LastResults == null)
                return;

            run.Check();
            bool solved = run.Session.IsSolved(run.LastResults);
            view.SetPrediction(solved, run.LastResults.Worst());
            view.ShowTrajectories(run.Session, run.LastResults, false);
            if (!solved)
                view.ShowBaseline(run.Session);
            view.PulseTrajectories();
            PublishReadout();
            if (solved && !solvedFired)
            {
                solvedFired = true;
                Solved?.Invoke();
            }
        }

        private void PublishReadout()
        {
            if (run.LastResults == null)
                return;

            view.ShowReadout(run.Readout());
        }

        private void ClearTimers()
        {
            nextPreviewTime = 0f;
            nextLatticeTime = 0f;
            nextReadoutTime = 0f;
        }

        public SpaceShapingSetup BuildSetup() =>
            SpaceShapingSetup.Build(currentPuzzleData, PuzzleSimSettings.From(settings));
    }
}
