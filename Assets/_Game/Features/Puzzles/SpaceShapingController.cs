using System;
using DigitalLove.Audio;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingController : MonoBehaviour
    {
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private SpaceShapingView view;
        [SerializeField] private FieldResetReadout resetReadout;
        [SerializeField] private FieldEdgeMeter meter;
        [SerializeField] private SynthCuePlayer cuePlayer;
        [SerializeField] private SynthPatch launchCue;

        private PuzzleRun run;
        private readonly PuzzlePreview preview = new();
        private bool wasScaling;
        private bool frozen;
        private PuzzleData currentPuzzleData;

        public event Action Solved;
        public event Action Failed;
        public event Action FlightCrashed;

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

        public event Action<ZoneRefusal> Refused
        {
            add => view.Refused += value;
            remove => view.Refused -= value;
        }

        public event Action ResetEnded
        {
            add => resetReadout.Ended += value;
            remove => resetReadout.Ended -= value;
        }

        public void Enter(PuzzleData puzzleData)
        {
            view.gameObject.SetActive(true);
            currentPuzzleData = puzzleData;
            meter.ShowJob(puzzleData.id);
            view.SetRoutePlan(RoutePlan.Build(puzzleData, settings.routePalette));
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
            view.SetRoutePlan(RoutePlan.Empty);
            frozen = false;
            run = null;
        }

        public float Glitch() => view.Glitch();

        public void PlayEntropyReset(int signpostIndex) => resetReadout.Play(signpostIndex);

        public float PlayArrival() => view.PlayArrival();

        public float PlayStamp(int grade) => view.PlayStamp(grade);

        public event Action<int> StarStamped
        {
            add => view.StarStamped += value;
            remove => view.StarStamped -= value;
        }

        public void HideAll() => view.gameObject.SetActive(false);

        public int Grade => run.Session.Grade();

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
            view.SetRoutePlan(RoutePlan.Build(currentPuzzleData, settings.routePalette));
            PuzzleFieldPreview.Show(BuildSetup(), view);
        }

        private void LateUpdate()
        {
            if (run == null || frozen || Time.timeScale <= 0f)
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
                preview.Reset();
            }

            run.PreviewZone(view.Zone);
            wasScaling = true;
            RenderPreview();
        }

        private void RenderPreview() => preview.Render(run, view);

        private void CommitScale()
        {
            wasScaling = false;
            run.CommitPreview();
            PushField();
            view.HideBaseline();
            view.TravelShips(run.Session, run.LastResults, true);
            cuePlayer.Play(launchCue);
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
            view.StopStamp();
            view.SoftResetShips(run.Session);
            PlayStartCheck();
            view.ShowBaseline(run.Session);
        }

        private void PreparePuzzle()
        {
            wasScaling = false;
            preview.Reset();
            ResetView();
        }

        private void PlayStartCheck()
        {
            PushField(false);
            view.TravelShips(run.Session, run.LastResults, false);
        }

        private void PushField(bool pendingPath = true)
        {
            view.StopShips();
            TrajectoryResult[] results = run.Simulate();
            PuzzlePreview.Apply(run, view);
            view.ShowTrajectories(run.Session, results, pendingPath);
            PublishReadout();
        }

        private void OnFlightCompleted()
        {
            if (run == null || frozen || run.LastResults == null)
                return;

            run.Check();
            bool solved = run.Session.IsSolved(run.LastResults);
            PublishFlight(solved);
            if (solved || run.Session.IsFailed(run.LastResults))
                FinishRun(solved);
            else if (run.LastResults.Worst() == TrajectoryOutcome.Crash)
                FlightCrashed?.Invoke();
        }

        private void PublishFlight(bool solved)
        {
            view.SetPrediction(solved, run.LastResults.Worst());
            view.ShowTrajectories(run.Session, run.LastResults, false);
            if (!solved)
                view.ShowBaseline(run.Session);
            view.PulseTrajectories();
            PublishReadout();
        }

        private void FinishRun(bool solved)
        {
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

        public SpaceShapingSetup BuildSetup() => SpaceShapingSetup.Build(currentPuzzleData, PuzzleSimSettings.From(settings));
    }
}
