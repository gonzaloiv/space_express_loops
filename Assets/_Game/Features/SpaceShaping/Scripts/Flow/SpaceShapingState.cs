using System;
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
        [SerializeField] private float clockPublishSecs = 0.1f;

        private readonly TrajectoryIntegrator integrator = new();
        private readonly ShipLaunch launch = new();

        private SpaceShapingSession session;
        private TrajectoryResult lastResult;
        private PuzzleReadout readout;
        private int puzzleIndex;
        private float[] debugSolvedScales;
        private bool wasScaling;
        private bool hasResult;
        private bool isChecked;
        private float nextClockPublish;

        public event Action<PuzzleReadout> ReadoutChanged;

        public PuzzleReadout Readout => readout;

        public override void Enter()
        {
            ship.Completed += OnFlightCompleted;
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
            view.Render(
                preview.BuildField(), preview.Zones, preview.Origin, preview.Destination,
                LatticeCrossHalf(preview));
        }

        public float DebugApplySolvedCube()
        {
            if (session == null)
                return 0f;

            ResetPuzzle();
            float scale = DebugScale(puzzleIndex);
            if (scale <= 0f)
                return 0f;

            view.SetZoneScale(scale);
            session.ApplyZone(view.Zone);
            ship.Travel(PushField());
            return scale;
        }

        private float DebugScale(int index)
        {
            if (debugSolvedScales == null)
                debugSolvedScales = new float[puzzles.Length];

            float scale = debugSolvedScales[index];
            if (scale > 0f)
                return scale;

            scale = FindDebugSolution(view.Zone);
            debugSolvedScales[index] = scale;
            return scale;
        }

        private float FindDebugSolution(SpaceZone zone) =>
            PuzzleDebugSolution.Find(BuildSetup(), zone, settings.minScaleFactor, settings.maxScaleFactor);

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

            if (session.IsOverBudget)
                return;

            if (view.HasChanged())
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
            view.ResetZone();
            PuzzleData puzzle = puzzles[puzzleIndex];
            view.SetConditions(ZoneConditions.From(puzzle));
            float size = puzzle.cubeSize > 0f ? puzzle.cubeSize : settings.cubeSize;
            view.SetZoneFrame(puzzle.zoneCentre, new Vector3(size, size, size), settings.zoneFalloff);
        }

        private void StartPuzzle()
        {
            wasScaling = false;
            isChecked = false;
            ResetView();
            session = new SpaceShapingSession(BuildSetup());
            ship.Reset(session.Origin);
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
            view.Render(
                field, session.Zones, session.Origin, session.Destination, LatticeCrossHalf(session));
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

        private float LatticeCrossHalf(SpaceShapingSession session)
        {
            Vector3 run = session.Destination - session.Origin;
            float length = run.magnitude;
            Vector3 direction = length > 0.0001f ? run / length : Vector3.forward;
            float half = 0f;
            for (int i = 0; i < session.SourceCount; i++)
            {
                Vector3 offset = session.GetPosition(i) - session.Origin;
                Vector3 perpendicular = offset - direction * Vector3.Dot(offset, direction);
                half = Mathf.Max(half, perpendicular.magnitude + session.GetScale(i) * 0.5f);
            }

            PuzzleData puzzle = puzzles[puzzleIndex];
            float cube = puzzle.cubeSize > 0f ? puzzle.cubeSize : settings.cubeSize;
            return Mathf.Max(half, cube * 0.5f);
        }

        private SpaceShapingSetup BuildSetup() =>
            SpaceShapingSetup.Build(puzzles[puzzleIndex], PuzzleSimSettings.From(settings));
    }

    internal static class PuzzleDebugSolution
    {
        public static float Find(SpaceShapingSetup setup, SpaceZone zone, float minScale, float maxScale)
        {
            const float step = 0.001f;
            SpaceShapingSession scratch = new(setup);
            TrajectoryIntegrator integrator = new();
            ShipLaunch launch = new();
            float bestScale = 0f;
            float bestCost = float.MaxValue;
            for (float scale = minScale; scale <= maxScale + step; scale += step)
            {
                scratch.Reset();
                scratch.ApplyZone(new SpaceZone(zone.Centre, zone.HalfExtents, scale, zone.Falloff));
                TrajectoryResult result = scratch.Simulate(scratch.BuildField(), integrator, launch);
                if (!scratch.IsSolved(result) || scratch.IsOverBudget)
                    continue;
                float cost = scratch.EntropyCommitted;
                if (cost >= bestCost)
                    continue;
                bestCost = cost;
                bestScale = scale;
            }

            return bestScale;
        }
    }
}
