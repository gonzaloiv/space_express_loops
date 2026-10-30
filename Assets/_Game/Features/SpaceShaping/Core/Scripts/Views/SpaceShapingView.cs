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
        [SerializeField] private FieldPlanetBehaviour destinationPrototype;
        [SerializeField] private Transform sourceParent;
        [SerializeField] private SpacetimeMeshView mesh;
        [SerializeField] private TrajectoryLineView trajectory;
        [SerializeField] private SpaceZoneView cubePrototype;
        [SerializeField] private Transform cubeParent;

        private const float FramePadding = 0.02f;

        private readonly List<FieldPlanetBehaviour> pool = new();
        private readonly List<SpaceZoneView> cubes = new();
        private readonly List<SpaceZone> lastZones = new();
        private FieldPlanetBehaviour destinationPlanet;
        private ZoneConditions conditions;
        private float zoneFalloff = 0.5f;
        private float minScale = 0.6f;
        private float maxScale = 1.8f;
        private bool hasSnapshot;
        private Vector3 frameCentre;
        private float frameHalf;

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
            int destination = DestinationIndex(session);
            int slot = 0;
            for (int i = 0; i < count; i++)
            {
                if (i == destination)
                    continue;

                Source(slot++).Spawn(session.GetPosition(i), session.GetScale(i) * 0.5f, i);
            }

            for (int i = slot; i < pool.Count; i++)
                pool[i].Hide();

            if (destination >= 0)
                destinationPlanet.Spawn(
                    session.GetPosition(destination), session.GetScale(destination) * 0.5f, destination);
            else
                destinationPlanet.Hide();
        }

        private static int DestinationIndex(SpaceShapingSession session)
        {
            for (int i = 0; i < session.SourceCount; i++)
            {
                if ((session.Home(i) - session.Destination).sqrMagnitude < 0.000001f)
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
            LatticeWarp warp = new(field, session.Zones);
            mesh.Render(warp.Warp, WarpStrength(session.Zones), frameCentre, frameHalf);
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
                Vector3 position = session.GetPosition(i);
                float radius = session.GetScale(i) * 0.5f;
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

        public void ShowTrajectory(TrajectoryResult result, bool solved) =>
            trajectory.Show(result, solved, frameCentre, frameHalf);

        public void Refuse()
        {
            SpaceZoneView cube = Active();
            if (cube != null)
                cube.Refuse();
        }

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
