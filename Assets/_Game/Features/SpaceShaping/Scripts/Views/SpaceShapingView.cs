using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingView : MonoBehaviour
    {
        [SerializeField] private FieldPlanetBehaviour prototype;
        [SerializeField] private Transform sourceParent;
        [SerializeField] private SpacetimeMeshView mesh;
        [SerializeField] private TrajectoryLineView trajectory;
        [SerializeField] private SpaceZoneView cubePrototype;
        [SerializeField] private Transform cubeParent;

        private const float FramePadding = 0.02f;

        private readonly List<FieldPlanetBehaviour> pool = new();
        private SpaceZoneView cube;
        private SpaceZone lastZone;
        private bool hasSnapshot;

        public SpaceZone Zone => cube.Zone;

        public bool IsScaling => cube.IsScaling;

        private void Awake()
        {
            cube = Instantiate(cubePrototype, cubeParent);
        }

        public bool HasChanged()
        {
            if (!hasSnapshot)
                return true;

            SpaceZone current = cube.Zone;
            return current.Centre != lastZone.Centre
                || current.HalfExtents != lastZone.HalfExtents
                || current.Scale != lastZone.Scale
                || current.Falloff != lastZone.Falloff;
        }

        public void ApplyFromSession(SpaceShapingSession session)
        {
            int count = session.SourceCount;
            for (int i = 0; i < count; i++)
                Source(i).Spawn(session.GetPosition(i), session.GetScale(i) * 0.5f, i);

            for (int i = count; i < pool.Count; i++)
                pool[i].Hide();
        }

        public void SetConditions(ZoneConditions conditions) => cube.SetConditions(conditions);

        public void SetZoneFrame(Vector3 centre, Vector3 size, float falloff)
        {
            cube.SetFrame(centre, size);
            cube.SetFalloff(falloff);
        }

        public void SetZoneScale(float scale) => cube.SetScale(scale);

        public void SetScaleEnvelope(float min, float max) => cube.SetScaleEnvelope(min, max);

        public void SetPrediction(bool solved, TrajectoryOutcome outcome) =>
            cube.SetPrediction(Verdict(solved, outcome));

        private static ZoneVerdict Verdict(bool solved, TrajectoryOutcome outcome)
        {
            if (solved)
                return ZoneVerdict.Solved;

            return outcome == TrajectoryOutcome.Crash ? ZoneVerdict.Crash : ZoneVerdict.Miss;
        }

        public void Snapshot()
        {
            lastZone = cube.Zone;
            hasSnapshot = true;
        }

        public void Render(SpacetimeField field, SpaceShapingSession session)
        {
            Frame(session, out Vector3 centre, out float half);
            mesh.Render(field, session.Zones, centre, half);
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

        public void ShowTrajectory(TrajectoryResult result, bool solved) => trajectory.Show(result, solved);

        public void Refuse() => cube.Refuse();

        public void PulseTrajectory() => trajectory.Pulse();

        public void ResetZone()
        {
            cube.gameObject.SetActive(true);
            cube.ResetScale();
            hasSnapshot = false;
        }

        private FieldPlanetBehaviour Source(int index)
        {
            if (pool.Count == 0)
                pool.AcquireInactive(prototype, sourceParent, created => created.Initialize());

            return pool.EnsureSlot(index, sourceParent, created => created.Initialize());
        }
    }
}
