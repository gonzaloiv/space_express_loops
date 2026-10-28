using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class TrajectoryLineView : MonoBehaviour
    {
        private const int MaxLinePoints = 400;

        [SerializeField] private LineRenderer line;
        [SerializeField] private Color arriveColor = new(0.35f, 1f, 0.55f, 1f);
        [SerializeField] private Color crashColor = new(1f, 0.35f, 0.3f, 1f);
        [SerializeField] private Color missColor = new(0.75f, 0.85f, 1f, 1f);

        public void Show(TrajectoryResult result)
        {
            int count = result.Points != null ? result.Points.Count : 0;
            if (count < 2)
            {
                Hide();
                return;
            }

            int stride = Mathf.Max(1, Mathf.CeilToInt((count - 1) / (float)(MaxLinePoints - 1)));
            int shown = (count - 1 + stride - 1) / stride + 1;
            line.positionCount = shown;

            int index = 0;
            for (int i = 0; i < count - 1; i += stride)
            {
                line.SetPosition(index, result.Points[i]);
                index++;
            }

            line.SetPosition(index, result.Points[count - 1]);
            SetColor(ColorFor(result.Outcome));
            line.enabled = true;
        }

        public void Hide()
        {
            line.positionCount = 0;
            line.enabled = false;
        }

        private Color ColorFor(TrajectoryOutcome outcome) => outcome switch
        {
            TrajectoryOutcome.Arrive => arriveColor,
            TrajectoryOutcome.Crash => crashColor,
            _ => missColor
        };

        private void SetColor(Color color)
        {
            line.startColor = color;
            line.endColor = color;
        }
    }
}
