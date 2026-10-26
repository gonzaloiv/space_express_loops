using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingView : MonoBehaviour
    {
        [SerializeField] private SpaceSourceView prototype;
        [SerializeField] private Transform sourceParent;
        [SerializeField] private SpacetimeMeshView mesh;
        [SerializeField] private SpaceZoneView zone;
        [SerializeField] private Transform origin;
        [SerializeField] private Transform destination;

        private readonly List<SpaceSourceView> pool = new();
        private SpaceZone lastZone;
        private bool hasSnapshot;

        public SpaceZone Zone => zone.Zone;

        public bool IsScaling => zone.IsScaling;

        public bool HasChanged()
        {
            if (!hasSnapshot)
                return true;

            SpaceZone current = zone.Zone;
            return current.Centre != lastZone.Centre
                || current.HalfExtents != lastZone.HalfExtents
                || current.Scale != lastZone.Scale
                || current.Falloff != lastZone.Falloff;
        }

        public void ApplyFromSession(SpaceShapingSession session)
        {
            SetEndpoints(session.Origin, session.Destination);

            int count = session.SourceCount;
            for (int i = 0; i < count; i++)
                Source(i).SetLocal(session.GetPosition(i), session.GetScale(i));

            for (int i = count; i < pool.Count; i++)
                pool[i].Hide();
        }

        public void SetEndpoints(Vector3 originPosition, Vector3 destinationPosition)
        {
            origin.localPosition = originPosition;
            destination.localPosition = destinationPosition;
        }

        public void SetZoneFrame(Vector3 size) => zone.SetFrame(Vector3.zero, size);

        public void Snapshot()
        {
            lastZone = zone.Zone;
            hasSnapshot = true;
        }

        public void Render(SpacetimeField field, IReadOnlyList<SpaceZone> zones) =>
            mesh.Render(field, zones, Vector3.Distance(origin.position, destination.position) * 0.5f);

        public void ResetZone()
        {
            zone.ResetScale();
            hasSnapshot = false;
        }

        private SpaceSourceView Source(int index)
        {
            if (pool.Count == 0)
                pool.AcquireInactive(prototype, sourceParent, created => created.Initialize());

            return pool.EnsureSlot(index, sourceParent, created => created.Initialize());
        }
    }
}
