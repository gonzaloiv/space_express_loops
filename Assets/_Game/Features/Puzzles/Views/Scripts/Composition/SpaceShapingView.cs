using System;
using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping.Cube;
using DigitalLove.Game.SpaceShaping.Grid;
using UnityEngine;
using UnityEngine.Serialization;

namespace DigitalLove.Game.SpaceShaping
{
    // Pure view: drives the collaborators owned by SpaceShapingSpawner and renders the lattice. It
    // does not instantiate anything itself.
    public class SpaceShapingView : MonoBehaviour
    {
        [SerializeField] private SpaceShapingSpawner spawner;
        [SerializeField] private SpacetimeMeshView mesh;
        [Tooltip("Minimum clearance between a planet's surface and the trajectory/ship at both caps, in metres.")]
        [FormerlySerializedAs("launchGap")]
        [SerializeField] private float capGap = 0.001f;
        [Tooltip("Clearance between a planet's surface and the trajectory/ship at both caps, as a fraction of the planet's radius. Keeps the gap visible on larger planets.")]
        [SerializeField] private float capGapFraction = 0.15f;
        [Tooltip("Lattice fade: distance from the nearest planet or the run where the fade starts, as a fraction of the field half-extent.")]
        [SerializeField] private float fadeNear = 0.15f;
        [Tooltip("Lattice fade: distance from the nearest planet or the run where the lattice is fully transparent, as a fraction of the field half-extent.")]
        [SerializeField] private float fadeFar = 0.55f;

        public event Action<ZoneRefusal> Refused
        {
            add => spawner.Zones.Refused += value;
            remove => spawner.Zones.Refused -= value;
        }

        public event Action ShipsCompleted
        {
            add => spawner.Ships.Completed += value;
            remove => spawner.Ships.Completed -= value;
        }

        public event Action<PuzzleReadout> ReadoutChanged;
        public event Action ResetRequested;

        public PuzzleReadout Readout { get; private set; }

        public PuzzleData CurrentPuzzle { get; private set; }

        public SpaceZone Zone => spawner.Zones.Zone;

        public bool IsScaling => spawner.Zones.IsScaling;

        public void ApplyFromSession(SpaceShapingSession session) => spawner.Planets.Apply(session);

        public void SetPuzzle(PuzzleData puzzle) => CurrentPuzzle = puzzle;

        public void ShowReadout(PuzzleReadout readout)
        {
            Readout = readout;
            ReadoutChanged?.Invoke(readout);
        }

        public void RequestReset() => ResetRequested?.Invoke();

        public void SetConditions(ZoneConditions value) => spawner.Zones.SetConditions(value);

        public void PlaceFrames(PuzzleFrame[] frames, float falloff) =>
            spawner.Zones.ShowFrames(frames, falloff);

        public void SetScaleEnvelope(float min, float max) =>
            spawner.Zones.SetScaleEnvelope(min, max);

        public void SetPrediction(bool solved, TrajectoryOutcome outcome) =>
            spawner.Zones.ApplyPrediction(Verdict(solved, outcome));

        private static ZoneVerdict Verdict(bool solved, TrajectoryOutcome outcome)
        {
            if (solved)
                return ZoneVerdict.Solved;

            return outcome == TrajectoryOutcome.Crash ? ZoneVerdict.Crash : ZoneVerdict.Miss;
        }

        public void Render(SpaceShapingSession session)
        {
            FieldFrame.Measure(session, out Vector3 centre, out float half);
            LatticeWarp warp = new(session.Zones);
            ContentFade fade = new(session, half * fadeNear, half * fadeFar);
            mesh.Render(warp.Warp, fade.Alpha, session.Zones.StrongestScaleOffset(), centre, half);
        }

        public void SoftResetShips(SpaceShapingSession session)
        {
            Vector3[] origins = new Vector3[session.ShipCount];
            for (int i = 0; i < origins.Length; i++)
                origins[i] = ShipRunGeometry.LaunchPoint(
                    session, session.Ship(i), capGap, capGapFraction);

            spawner.Ships.SoftReset(origins);
        }

        public void StopShips() => spawner.Ships.Stop();

        public void TravelShips(SpaceShapingSession session, IReadOnlyList<TrajectoryResult> results) =>
            spawner.Ships.Travel(
                ShipRunGeometry.Visible(session, results, capGap, capGapFraction),
                session.StepSecs);

        public void ShowTrajectories(
            SpaceShapingSession session, IReadOnlyList<TrajectoryResult> results, bool pending)
        {
            TrajectoryResult[] visible = VisibleRuns(session, results, out Vector3 centre, out float half);
            spawner.Lines.Show(visible, centre, half, pending);
        }

        private TrajectoryResult[] VisibleRuns(
            SpaceShapingSession session, IReadOnlyList<TrajectoryResult> results,
            out Vector3 centre, out float half)
        {
            TrajectoryResult[] visible = ShipRunGeometry.Visible(
                session, results, capGap, capGapFraction);
            FieldFrame.Measure(session, out centre, out half);
            for (int i = 0; i < visible.Length; i++)
                visible[i] = TrajectoryCaps.ClipToBox(visible[i], centre, half);

            return visible;
        }

        public void ShowBaseline(SpaceShapingSession session)
        {
            int count = session.ShipCount;
            Vector3[] starts = new Vector3[count];
            Vector3[] ends = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                ShipSpec ship = session.Ship(i);
                starts[i] = ShipRunGeometry.LaunchPoint(session, ship, capGap, capGapFraction);
                ends[i] = ShipRunGeometry.ArrivalPoint(session, ship, capGap, capGapFraction);
            }

            spawner.Baselines.ShowBaselines(starts, ends);
        }

        public void HideBaseline() => spawner.Baselines.Hide();

        public void HideTrajectories() => spawner.Lines.Hide();

        public void PulseTrajectories() => spawner.Lines.Pulse();

        public void ClearPrediction() => spawner.Zones.ClearPredictions();

        public void Refuse(ZoneRefusal reason) => spawner.Zones.Refuse(reason);
    }
}
