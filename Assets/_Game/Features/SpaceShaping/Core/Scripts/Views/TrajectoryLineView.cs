using System.Collections;
using System.Collections.Generic;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class TrajectoryLineView : MonoBehaviour
    {
        private const int MaxLinePoints = 400;
        private const float PulseSecs = 0.35f;
        private const float HeadingEpsilon = 0.000001f;

        [SerializeField] private LineRenderer line;
        [SerializeField] private LineRenderer headingLine;
        [SerializeField] private Transform endMarker;
        [SerializeField] private Renderer endMarkerRenderer;
        [SerializeField] private float lineWidth = 0.008f;
        [SerializeField] private float headingLength = 3f;
        [SerializeField] private Color arriveColor = new(0.35f, 1f, 0.55f, 1f);
        [SerializeField] private Color crashColor = new(1f, 0.35f, 0.3f, 1f);
        [SerializeField] private Color missColor = new(0.75f, 0.85f, 1f, 1f);

        private RendererTint markerTint;
        private Coroutine pulse;
        private Vector3 markerBaseScale;
        private bool hasMarkerScale;

        private void Awake()
        {
            CaptureMarkerScale();
            line.widthMultiplier = lineWidth;
            headingLine.widthMultiplier = lineWidth;
        }

        public void Show(TrajectoryResult result, bool solved)
        {
            StopPulse();
            int count = result.Points != null ? result.Points.Count : 0;
            if (count < 2)
            {
                Hide();
                return;
            }

            FillLine(result.Points);
            Color color = solved ? arriveColor : ColorFor(result.Outcome);
            SetColor(color);
            Vector3 end = result.Points[count - 1];
            Vector3 heading = Heading(result.Points[count - 2], end);
            ShowMarker(end, heading, color);
            ShowHeading(end, heading, color);
            line.enabled = true;
        }

        private void FillLine(IReadOnlyList<Vector3> points)
        {
            int count = points.Count;
            int stride = Mathf.Max(1, Mathf.CeilToInt((count - 1) / (float)(MaxLinePoints - 1)));
            int shown = (count - 1 + stride - 1) / stride + 1;
            line.positionCount = shown;

            int index = 0;
            for (int i = 0; i < count - 1; i += stride)
            {
                line.SetPosition(index, points[i]);
                index++;
            }

            line.SetPosition(index, points[count - 1]);
        }

        private void ShowMarker(Vector3 position, Vector3 heading, Color color)
        {
            SetMarkerColor(color);
            endMarker.localPosition = position;
            if (heading.sqrMagnitude > HeadingEpsilon)
                endMarker.localRotation = Quaternion.LookRotation(heading, Vector3.up);
            endMarker.gameObject.SetActive(true);
        }

        private void ShowHeading(Vector3 end, Vector3 heading, Color color)
        {
            if (heading.sqrMagnitude < HeadingEpsilon)
            {
                headingLine.enabled = false;
                return;
            }

            headingLine.positionCount = 2;
            headingLine.SetPosition(0, end);
            headingLine.SetPosition(1, end + heading * headingLength);
            headingLine.startColor = color;
            headingLine.endColor = color;
            headingLine.enabled = true;
        }

        private static Vector3 Heading(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            return delta.sqrMagnitude < HeadingEpsilon ? Vector3.zero : delta.normalized;
        }

        public void Hide()
        {
            StopPulse();
            line.positionCount = 0;
            line.enabled = false;
            headingLine.positionCount = 0;
            headingLine.enabled = false;
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

        private Color ColorFor(TrajectoryOutcome outcome) =>
            outcome == TrajectoryOutcome.Crash ? crashColor : missColor;

        private void SetColor(Color color)
        {
            line.startColor = color;
            line.endColor = color;
        }

        private void SetMarkerColor(Color color) => markerTint.Set(endMarkerRenderer, color);
    }
}
