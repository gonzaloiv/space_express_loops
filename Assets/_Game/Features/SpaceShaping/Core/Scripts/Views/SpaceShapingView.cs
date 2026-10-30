using System;
using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Game.SpaceShaping.Cube;
using DigitalLove.Game.SpaceShaping.Grid;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingView : MonoBehaviour
    {
        [SerializeField] private FieldPlanetBehaviour prototype;
        [SerializeField] private FieldPlanetBehaviour originPrototype;
        [SerializeField] private FieldPlanetBehaviour destinationPrototype;
        [SerializeField] private Transform sourceParent;
        [SerializeField] private SpacetimeMeshView mesh;
        [SerializeField] private LatticeDustView dust;
        [SerializeField] private TrajectoryLineView trajectory;
        [SerializeField] private SpaceZoneView cubePrototype;
        [SerializeField] private Transform cubeParent;
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

        private const float FramePadding = 0.02f;

        private readonly List<FieldPlanetBehaviour> pool = new();
        private readonly List<SpaceZoneView> cubes = new();
        private readonly List<SpaceZone> lastZones = new();
        private FieldPlanetBehaviour originPlanet;
        private FieldPlanetBehaviour destinationPlanet;
        private ZoneConditions conditions;
        private float zoneFalloff = 0.5f;
        private float minScale = 0.6f;
        private float maxScale = 1.8f;
        private bool hasSnapshot;
        private Vector3 frameCentre;
        private float frameHalf;
        private bool dustPlaced;
        private float dustHalf;

        public SpaceZone Zone
        {
            get
            {
                SpaceZoneView cube = Active();
                return cube != null ? cube.Zone : default;
            }
        }

        public bool IsScaling
        {
            get
            {
                for (int i = 0; i < cubes.Count; i++)
                {
                    if (cubes[i].IsScaling)
                        return true;
                }

                return false;
            }
        }

        private void Awake()
        {
            originPlanet = Instantiate(originPrototype, sourceParent);
            originPlanet.Initialize();
            destinationPlanet = Instantiate(destinationPrototype, sourceParent);
            destinationPlanet.Initialize();
        }

        public bool HasChanged()
        {
            if (!hasSnapshot || lastZones.Count != cubes.Count)
                return true;

            for (int i = 0; i < cubes.Count; i++)
            {
                if (Changed(cubes[i].Zone, lastZones[i]))
                    return true;
            }

            return false;
        }

        private static bool Changed(SpaceZone current, SpaceZone last) =>
            current.Centre != last.Centre
            || current.HalfExtents != last.HalfExtents
            || current.Scale != last.Scale
            || current.Falloff != last.Falloff;

        public void ApplyFromSession(SpaceShapingSession session)
        {
            int count = session.SourceCount;
            int origin = IndexAt(session, session.Origin);
            int destination = IndexAt(session, session.Destination);
            int slot = 0;
            for (int i = 0; i < count; i++)
            {
                if (i == origin || i == destination)
                    continue;

                Source(slot++).Spawn(session.GetPosition(i), session.GetScale(i), i);
            }

            for (int i = slot; i < pool.Count; i++)
                pool[i].Hide();

            Spawn(originPlanet, session, origin);
            Spawn(destinationPlanet, session, destination);
        }

        private static void Spawn(FieldPlanetBehaviour planet, SpaceShapingSession session, int index)
        {
            if (index >= 0)
                planet.Spawn(session.GetPosition(index), session.GetScale(index), index);
            else
                planet.Hide();
        }

        private static int IndexAt(SpaceShapingSession session, Vector3 point)
        {
            for (int i = 0; i < session.SourceCount; i++)
            {
                if ((session.Home(i) - point).sqrMagnitude < 0.000001f)
                    return i;
            }

            return -1;
        }

        public void SetConditions(ZoneConditions value)
        {
            conditions = value;
            ForEachCube(cube => cube.SetConditions(value));
        }

        public void PlaceFrames(PuzzleFrame[] frames, float falloff)
        {
            zoneFalloff = falloff;
            int count = frames != null ? frames.Length : 0;
            for (int i = 0; i < count; i++)
                Place(Cube(i), frames[i]);

            for (int i = count; i < cubes.Count; i++)
                cubes[i].gameObject.SetActive(false);

            hasSnapshot = false;
            dustPlaced = false;
        }

        public void SetScaleEnvelope(float min, float max)
        {
            minScale = min;
            maxScale = max;
            ForEachCube(cube => cube.SetScaleEnvelope(min, max));
        }

        public void SetPrediction(bool solved, TrajectoryOutcome outcome)
        {
            ZoneVerdict verdict = Verdict(solved, outcome);
            ForEachCube(cube => cube.SetPrediction(verdict));
        }

        private static ZoneVerdict Verdict(bool solved, TrajectoryOutcome outcome)
        {
            if (solved)
                return ZoneVerdict.Solved;

            return outcome == TrajectoryOutcome.Crash ? ZoneVerdict.Crash : ZoneVerdict.Miss;
        }

        public void Snapshot()
        {
            lastZones.Clear();
            for (int i = 0; i < cubes.Count; i++)
                lastZones.Add(cubes[i].Zone);

            hasSnapshot = true;
        }

        public void Render(SpacetimeField field, SpaceShapingSession session)
        {
            Frame(session, out frameCentre, out frameHalf);
            LatticeWarp warp = new(field, session.Zones, warpDepth, warpMax);
            ContentFade fade = new(session, frameHalf * fadeNear, frameHalf * fadeFar);
            mesh.Render(warp.Warp, fade.Alpha, WarpStrength(session.Zones), frameCentre, frameHalf);
            RenderDust(warp.Warp, session);
        }

        private void RenderDust(Func<Vector3, Vector3> warp, SpaceShapingSession session)
        {
            if (!dustPlaced)
            {
                dustHalf = frameHalf;
                dustPlaced = true;
            }

            dust.Render(warp, session.ZoneCentre, dustHalf);
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

        private static void Frame(SpaceShapingSession session, out Vector3 centre, out float half)
        {
            Vector3 min = session.Origin;
            Vector3 max = session.Origin;
            Encapsulate(ref min, ref max, session.Destination);
            for (int i = 0; i < session.SourceCount; i++)
            {
                Vector3 position = session.Home(i);
                float radius = session.HomeScale(i) * 0.5f;
                Encapsulate(ref min, ref max, position - Vector3.one * radius);
                Encapsulate(ref min, ref max, position + Vector3.one * radius);
            }

            centre = (min + max) * 0.5f;
            Vector3 size = max - min;
            half = Mathf.Max(Mathf.Max(size.x, size.y), size.z) * 0.5f + FramePadding;
        }

        private static void Encapsulate(ref Vector3 min, ref Vector3 max, Vector3 point)
        {
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }

        public Vector3 LaunchPoint(SpaceShapingSession session)
        {
            Vector3 direction = session.Destination - session.Origin;
            direction = direction.sqrMagnitude > 0.000001f ? direction.normalized : Vector3.forward;
            return session.Origin + direction * (session.OriginRadius + launchGap);
        }

        public TrajectoryResult ClipToLaunch(TrajectoryResult result, SpaceShapingSession session)
        {
            if (!result.HasPoints)
                return result;

            float clearance = session.OriginRadius + launchGap;
            float clearanceSq = clearance * clearance;
            int start = 0;
            while (start < result.Points.Count
                && (result.Points[start] - session.Origin).sqrMagnitude < clearanceSq)
                start++;

            if (start == 0 || start >= result.Points.Count)
                return result;

            return new TrajectoryResult(
                result.Outcome, result.Points.GetRange(start, result.Points.Count - start));
        }

        public void ShowTrajectory(TrajectoryResult result, bool solved) =>
            trajectory.Show(result, solved, frameCentre, frameHalf);

        public void ClearPrediction() => ForEachCube(cube => cube.ClearPrediction());

        public event Action<ZoneRefusal> Refused;

        public void Refuse(ZoneRefusal reason)
        {
            SpaceZoneView cube = Active();
            if (cube != null)
                cube.Refuse(reason);
        }

        private void OnCubeRefused(ZoneRefusal reason) => Refused?.Invoke(reason);

        public void PulseTrajectory() => trajectory.Pulse();

        private void ForEachCube(Action<SpaceZoneView> apply)
        {
            for (int i = 0; i < cubes.Count; i++)
                apply(cubes[i]);
        }

        private SpaceZoneView Active()
        {
            for (int i = 0; i < cubes.Count; i++)
            {
                if (cubes[i].IsScaling)
                    return cubes[i];
            }

            return cubes.Count > 0 ? cubes[0] : null;
        }

        private SpaceZoneView Cube(int index)
        {
            while (index >= cubes.Count)
                cubes.Add(Create());

            return cubes[index];
        }

        private SpaceZoneView Create()
        {
            SpaceZoneView cube = Instantiate(cubePrototype, cubeParent);
            cube.SetConditions(conditions);
            cube.SetScaleEnvelope(minScale, maxScale);
            cube.Refused += OnCubeRefused;
            return cube;
        }

        private void Place(SpaceZoneView cube, PuzzleFrame frame)
        {
            cube.gameObject.SetActive(true);
            cube.ResetScale();
            cube.SetFrame(frame.centre, frame.BoxSize);
            cube.SetFalloff(zoneFalloff);
        }

        private FieldPlanetBehaviour Source(int index)
        {
            if (pool.Count == 0)
                pool.AcquireInactive(prototype, sourceParent, created => created.Initialize());

            return pool.EnsureSlot(index, sourceParent, created => created.Initialize());
        }
    }
}
