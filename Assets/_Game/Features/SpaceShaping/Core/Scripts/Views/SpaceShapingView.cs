using System;
using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping.Cube;
using DigitalLove.Game.SpaceShaping.Grid;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Pure view: drives the views owned by SpaceShapingSpawner and renders the lattice. It does
    // not instantiate anything itself.
    public class SpaceShapingView : MonoBehaviour
    {
        [SerializeField] private SpaceShapingSpawner spawner;
        [SerializeField] private SpacetimeMeshView mesh;
        [Tooltip("Visual gap between the origin planet's surface and where the trajectory and ship start, in metres.")]
        [SerializeField] private float launchGap = 0.02f;
        [Tooltip("Lattice fade: distance from the nearest planet or the run where the fade starts, as a fraction of the field half-extent.")]
        [SerializeField] private float fadeNear = 0.15f;
        [Tooltip("Lattice fade: distance from the nearest planet or the run where the lattice is fully transparent, as a fraction of the field half-extent.")]
        [SerializeField] private float fadeFar = 0.55f;
        [Tooltip("Lattice warp: how deep the wells pull the lattice, as a multiplier on the field pull.")]
        [SerializeField] private float warpDepth = 0.3f;
        [Tooltip("Lattice warp: the maximum well displacement, in metres.")]
        [SerializeField] private float warpMax = 0.16f;

        private ZoneConditions conditions;
        private float zoneFalloff = 0.5f;
        private float minScale = 0.6f;
        private float maxScale = 1.8f;
        private Vector3 frameCentre;
        private float frameHalf;

        public event Action<ZoneRefusal> Refused
        {
            add => spawner.CubeRefused += value;
            remove => spawner.CubeRefused -= value;
        }

        public event Action ShipsCompleted
        {
            add => spawner.ShipsCompleted += value;
            remove => spawner.ShipsCompleted -= value;
        }

        public SpaceZone Zone
        {
            get
            {
                SpaceZoneView cube = Active();
                return cube != null ? cube.Zone : default;
            }
        }

        public bool IsScaling => spawner.Cubes.AnyScaling();

        public void ApplyFromSession(SpaceShapingSession session) => spawner.SpawnPlanets(session);

        public void SetConditions(ZoneConditions value)
        {
            conditions = value;
            spawner.Cubes.ApplyConditions(value);
        }

        public void PlaceFrames(PuzzleFrame[] frames, float falloff)
        {
            zoneFalloff = falloff;
            int count = frames != null ? frames.Length : 0;
            for (int i = 0; i < count; i++)
                Place(Cube(i), frames[i]);

            spawner.HideCubesFrom(count);
        }

        public void SetScaleEnvelope(float min, float max)
        {
            minScale = min;
            maxScale = max;
            spawner.Cubes.ApplyScaleEnvelope(min, max);
        }

        public void SetPrediction(bool solved, TrajectoryOutcome outcome) =>
            spawner.Cubes.ApplyPrediction(Verdict(solved, outcome));

        private static ZoneVerdict Verdict(bool solved, TrajectoryOutcome outcome)
        {
            if (solved)
                return ZoneVerdict.Solved;

            return outcome == TrajectoryOutcome.Crash ? ZoneVerdict.Crash : ZoneVerdict.Miss;
        }

        public void Render(SpacetimeField field, SpaceShapingSession session)
        {
            FieldFrame.Measure(session, out frameCentre, out frameHalf);
            LatticeWarp warp = new(field, session.Zones, warpDepth, warpMax);
            ContentFade fade = new(session, frameHalf * fadeNear, frameHalf * fadeFar);
            mesh.Render(warp.Warp, fade.Alpha, WarpStrength(session.Zones), frameCentre, frameHalf);
        }

        private static float WarpStrength(IReadOnlyList<SpaceZone> zones)
        {
            float strongest = 1f;
            for (int i = 0; i < zones.Count; i++)
            {
                if (Mathf.Abs(zones[i].Scale - 1f) > Mathf.Abs(strongest - 1f))
                    strongest = zones[i].Scale;
            }

            return strongest - 1f;
        }

        public void SoftResetShips(SpaceShapingSession session)
        {
            Vector3[] origins = new Vector3[session.ShipCount];
            for (int i = 0; i < origins.Length; i++)
                origins[i] = LaunchPoint(session, session.Ship(i));

            spawner.Ships.SoftReset(origins);
        }

        public void StopShips() => spawner.Ships.Stop();

        public void TravelShips(SpaceShapingSession session, IReadOnlyList<TrajectoryResult> results) =>
            spawner.Ships.Travel(Visible(session, results), session.StepSecs);

        public void ShowTrajectories(
            SpaceShapingSession session, IReadOnlyList<TrajectoryResult> results, bool pending) =>
            spawner.Lines.Show(Visible(session, results), frameCentre, frameHalf, pending);

        public void PulseTrajectories() => spawner.Lines.Pulse();

        private TrajectoryResult[] Visible(
            SpaceShapingSession session, IReadOnlyList<TrajectoryResult> results)
        {
            TrajectoryResult[] visible = new TrajectoryResult[results.Count];
            for (int i = 0; i < results.Count; i++)
                visible[i] = ClipToLaunch(results[i], session, session.Ship(i));

            return visible;
        }

        private Vector3 LaunchPoint(SpaceShapingSession session, ShipSpec ship)
        {
            Vector3 direction = ship.Destination - ship.Origin;
            direction = direction.sqrMagnitude > 0.000001f ? direction.normalized : Vector3.forward;
            return ship.Origin + direction * (session.RadiusAt(ship.Origin) + launchGap);
        }

        private TrajectoryResult ClipToLaunch(
            TrajectoryResult result, SpaceShapingSession session, ShipSpec ship)
        {
            if (!result.HasPoints)
                return result;

            float clearance = session.RadiusAt(ship.Origin) + launchGap;
            float clearanceSq = clearance * clearance;
            int start = 0;
            while (start < result.Points.Count
                && (result.Points[start] - ship.Origin).sqrMagnitude < clearanceSq)
                start++;

            if (start == 0 || start >= result.Points.Count)
                return result;

            return new TrajectoryResult(
                result.Outcome, result.Points.GetRange(start, result.Points.Count - start));
        }

        public void ClearPrediction() => spawner.Cubes.ClearPredictions();

        public void Refuse(ZoneRefusal reason)
        {
            SpaceZoneView cube = Active();
            if (cube != null)
                cube.Refuse(reason);
        }

        private SpaceZoneView Active() => spawner.Cubes.Active();

        private SpaceZoneView Cube(int index)
        {
            SpaceZoneView cube = spawner.Cube(index);
            cube.SetConditions(conditions);
            cube.SetScaleEnvelope(minScale, maxScale);
            return cube;
        }

        private void Place(SpaceZoneView cube, PuzzleFrame frame)
        {
            cube.gameObject.SetActive(true);
            cube.ResetScale();
            cube.SetFrame(frame.centre, frame.BoxSize);
            cube.SetFalloff(zoneFalloff);
        }
    }
}
