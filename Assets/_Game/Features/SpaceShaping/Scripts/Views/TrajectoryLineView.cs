using System.Collections;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class TrajectoryLineView : MonoBehaviour
    {
        private const int MaxLinePoints = 400;
        private const float PulseSecs = 0.35f;

        [SerializeField] private LineRenderer line;
        [SerializeField] private Transform endMarker;
        [SerializeField] private Renderer endMarkerRenderer;
        [SerializeField] private Color arriveColor = new(0.35f, 1f, 0.55f, 1f);
        [SerializeField] private Color crashColor = new(1f, 0.35f, 0.3f, 1f);
        [SerializeField] private Color missColor = new(0.75f, 0.85f, 1f, 1f);

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock markerBlock;
        private Coroutine pulse;
        private Vector3 markerBaseScale;
        private bool hasMarkerScale;

        private void Awake() => CaptureMarkerScale();

        public void Show(TrajectoryResult result)
        {
            StopPulse();
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
            Color color = ColorFor(result.Outcome);
            SetColor(color);
            SetMarkerColor(color);
            endMarker.localPosition = result.Points[count - 1];
            endMarker.gameObject.SetActive(true);
            line.enabled = true;
        }

        public void Hide()
        {
            StopPulse();
            line.positionCount = 0;
            line.enabled = false;
            endMarker.gameObject.SetActive(false);
        }

        public void Pulse()
        {
            if (!endMarker.gameObject.activeSelf)
                return;

            StopPulse();
            pulse = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            float t = 0f;
            while (t < PulseSecs)
            {
                t += Time.deltaTime;
                float bump = 1f + Mathf.Sin(Mathf.Clamp01(t / PulseSecs) * Mathf.PI) * 0.8f;
                endMarker.localScale = markerBaseScale * bump;
                yield return null;
            }

            endMarker.localScale = markerBaseScale;
            pulse = null;
        }

        private void StopPulse()
        {
            CaptureMarkerScale();
            if (pulse != null)
                StopCoroutine(pulse);

            pulse = null;
            endMarker.localScale = markerBaseScale;
        }

        private void CaptureMarkerScale()
        {
            if (hasMarkerScale)
                return;

            markerBaseScale = endMarker.localScale;
            hasMarkerScale = true;
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

        private void SetMarkerColor(Color color)
        {
            markerBlock ??= new MaterialPropertyBlock();
            endMarkerRenderer.GetPropertyBlock(markerBlock);
            markerBlock.SetColor(BaseColorId, color);
            markerBlock.SetColor(ColorId, color);
            endMarkerRenderer.SetPropertyBlock(markerBlock);
        }
    }
}
