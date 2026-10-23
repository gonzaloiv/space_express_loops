using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class TrajectoryPredictionView : MonoBehaviour
    {
        [SerializeField] private LineRenderer line;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock propertyBlock;

        public void Render(TrajectoryResult result, Vector3 destinationLocal)
        {
            ApplyColor(result.Outcome);
            bool arrived = result.Outcome == TrajectoryOutcome.Arrive;
            int count = result.Points.Count;
            line.positionCount = arrived ? count + 1 : count;
            for (int i = 0; i < count; i++)
                line.SetPosition(i, result.Points[i]);
            if (arrived)
                line.SetPosition(count, destinationLocal);
        }

        private void ApplyColor(TrajectoryOutcome outcome)
        {
            propertyBlock ??= new MaterialPropertyBlock();
            line.GetPropertyBlock(propertyBlock);
            Color color = ColorFor(outcome);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(ColorId, color);
            line.SetPropertyBlock(propertyBlock);
        }

        private static Color ColorFor(TrajectoryOutcome outcome)
        {
            if (outcome == TrajectoryOutcome.Arrive)
                return new Color(0.30f, 1f, 0.40f, 1f);
            if (outcome == TrajectoryOutcome.Crash)
                return new Color(1f, 0.28f, 0.24f, 1f);
            return new Color(1f, 0.80f, 0.25f, 1f);
        }
    }
}
