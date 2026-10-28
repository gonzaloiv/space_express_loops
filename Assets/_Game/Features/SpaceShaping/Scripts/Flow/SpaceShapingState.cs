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
            view.Render(preview.BuildField(), preview.Zones, FieldRadius(preview));
        }

        public float DebugApplySolvedCube()
        {
            if (session == null)
                return 0f;

            ResetPuzzle();
            float scale = FindDebugSolution(view.Zone);
            if (scale <= 0f)
                return 0f;

            view.SetZoneScale(scale);
            session.ApplyZone(view.Zone);
            ship.Travel(PushField());
            return scale;
        }

        private float FindDebugSolution(SpaceZone zone)
        {
            const float step = 0.001f;
            SpaceShapingSession scratch = new(BuildSetup());
            float bestScale = 0f;
            float bestCost = float.MaxValue;
            for (float scale = settings.minScaleFactor; scale <= settings.maxScaleFactor + step; scale += step)
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

        private void LateUpdate()
        {
            if (session == null || !parent.IsCurrentState<SpaceShapingState>())
                return;

            TrackGesture();
        }

        private void TrackGesture()
        {
            if (view.IsScaling)
            {
                if (!wasScaling && session.IsOverBudget)
                    return;

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
            view.Render(field, session.Zones, FieldRadius(session));
            view.ShowTrajectory(result);
            view.Snapshot();

            lastResult = result;
            hasResult = true;
            isChecked = false;
            PublishReadout();
            return result;
        }

        private void OnFlightCompleted()
        {
            if (session == null || !hasResult)
                return;

            isChecked = true;
            PublishReadout();
        }

        private void PublishReadout()
        {
            if (!hasResult)
                return;

            readout = PuzzleReadout.From(session, lastResult, isChecked);
            ReadoutChanged?.Invoke(readout);
        }

        private static float FieldRadius(SpaceShapingSession session)
        {
            float outer = 0f;
            for (int i = 0; i < session.SourceCount; i++)
                outer = Mathf.Max(outer, session.GetPosition(i).magnitude + session.GetScale(i) * 0.5f);

            return Mathf.Max(outer, 0.01f);
        }

        private SpaceShapingSetup BuildSetup() =>
            SpaceShapingSetup.Build(puzzles[puzzleIndex], PuzzleSimSettings.From(settings));
    }
}
