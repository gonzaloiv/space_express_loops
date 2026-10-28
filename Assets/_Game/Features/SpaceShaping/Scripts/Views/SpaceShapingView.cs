using System.Collections.Generic;
using DigitalLove.Game.Nodes;
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

        private readonly List<FieldPlanetBehaviour> pool = new();
        private SpaceZoneView cube;
        private SpaceZone lastZone;
        private bool hasSnapshot;

        public SpaceZone Zone => Cube().Zone;

        public bool IsScaling => Cube().IsScaling;

        public bool HasChanged()
        {
            if (!hasSnapshot)
                return true;

            SpaceZone current = Cube().Zone;
            return current.Centre != lastZone.Centre
                || current.HalfExtents != lastZone.HalfExtents
                || current.Scale != lastZone.Scale
                || current.Falloff != lastZone.Falloff;
        }

        public void ApplyFromSession(SpaceShapingSession session)
        {
            int count = session.SourceCount;
            for (int i = 0; i < count; i++)
                Source(i).Spawn(session.GetPosition(i), session.GetScale(i), i);

            for (int i = count; i < pool.Count; i++)
                pool[i].Hide();
        }

        public void SetConditions(ZoneConditions conditions) => Cube().SetConditions(conditions);

        public void SetZoneFrame(Vector3 centre, Vector3 size, float falloff)
        {
            Cube().SetFrame(centre, size);
            Cube().SetFalloff(falloff);
        }

        public void SetZoneScale(float scale) => Cube().SetScale(scale);

        public void SetPrediction(bool solved, TrajectoryOutcome outcome) =>
            Cube().SetPrediction(solved, outcome);

        public void Snapshot()
        {
            lastZone = Cube().Zone;
            hasSnapshot = true;
        }

        public void Render(
            SpacetimeField field, IReadOnlyList<SpaceZone> zones, Vector3 origin, Vector3 destination,
            float crossHalf) =>
            mesh.Render(field, zones, origin, destination, crossHalf);

        public void ShowTrajectory(TrajectoryResult result) => trajectory.Show(result);

        public void PulseTrajectory() => trajectory.Pulse();

        public void ResetZone()
        {
            Cube().ResetScale();
            hasSnapshot = false;
        }

        private SpaceZoneView Cube()
        {
            if (cube == null && Application.isPlaying)
                cube = Instantiate(cubePrototype, cubeParent);

            return cube;
        }

        private FieldPlanetBehaviour Source(int index)
        {
            if (pool.Count == 0)
                pool.AcquireInactive(prototype, sourceParent, created => created.Initialize());

            return pool.EnsureSlot(index, sourceParent, created => created.Initialize());
        }
    }
}
