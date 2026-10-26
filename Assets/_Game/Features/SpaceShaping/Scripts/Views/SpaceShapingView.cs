using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingView : MonoBehaviour
    {
        [SerializeField] private SpaceSourceView[] sources;
        [SerializeField] private SpacetimeMeshView mesh;
        [SerializeField] private TrajectoryPredictionView prediction;
        [SerializeField] private SpaceZoneView zone;
        [SerializeField] private Transform origin;
        [SerializeField] private Transform destination;

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
            origin.localPosition = session.Origin;
            destination.localPosition = session.Destination;

            int count = Mathf.Min(sources.Length, session.SourceCount);
            for (int i = 0; i < count; i++)
                sources[i].SetLocal(session.GetPosition(i), session.GetScale(i));
        }

        public void Snapshot()
        {
            lastZone = zone.Zone;
            hasSnapshot = true;
        }

        public void Render(
            SpacetimeField field, TrajectoryResult result, Vector3 destinationLocal, IReadOnlyList<SpaceZone> zones)
        {
            mesh.Render(field, zones);
            prediction.Render(result, destinationLocal);
        }

        public void SetPredictionVisible(bool visible) => prediction.SetVisible(visible);

        public void ResetZone()
        {
            zone.ResetScale();
            hasSnapshot = false;
        }
    }
}
