using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingView : MonoBehaviour
    {
        [SerializeField] private SpaceSourceView[] sources;
        [SerializeField] private SpacetimeGridView grid;
        [SerializeField] private SpacetimeMeshView mesh;
        [SerializeField] private TrajectoryPredictionView prediction;

        private Vector3[] lastPositions;
        private float[] lastScales;
        private bool hasSnapshot;

        public bool HasChanged()
        {
            if (!hasSnapshot || lastPositions == null || lastPositions.Length != sources.Length)
                return true;

            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i].LocalPosition != lastPositions[i] || sources[i].LocalScale != lastScales[i])
                    return true;
            }

            return false;
        }

        public void SyncToSession(SpaceShapingSession session)
        {
            for (int i = 0; i < sources.Length; i++)
                session.SetSource(i, sources[i].LocalPosition, sources[i].LocalScale);
        }

        public void ApplyFromSession(SpaceShapingSession session)
        {
            for (int i = 0; i < sources.Length; i++)
                sources[i].SetLocal(session.GetPosition(i), session.GetScale(i));
        }

        public void Snapshot()
        {
            if (lastPositions == null || lastPositions.Length != sources.Length)
            {
                lastPositions = new Vector3[sources.Length];
                lastScales = new float[sources.Length];
            }

            for (int i = 0; i < sources.Length; i++)
            {
                lastPositions[i] = sources[i].LocalPosition;
                lastScales[i] = sources[i].LocalScale;
            }

            hasSnapshot = true;
        }

        public void Render(SpacetimeField field, TrajectoryResult result, Vector3 destinationLocal)
        {
            grid.Render(field);
            mesh.Render(field);
            prediction.Render(result, destinationLocal);
        }
    }
}
