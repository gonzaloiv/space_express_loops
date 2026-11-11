using System.Collections;
using System.Collections.Generic;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class TrajectoryLineView : MonoBehaviour
    {
        private const int MaxLinePoints = 400;
        private const float PulseSecs = 0.5f;
        private const float HeadingEpsilon = 1e-10f;
        private const float TailDim = 0.2f;

        [SerializeField] private LineRenderer line;
        [SerializeField] private LineRenderer headingLine;
        [SerializeField] private Transform endMarker;
        [SerializeField] private Renderer endMarkerRenderer;
        [SerializeField] private float baselineWidth = 0.0016f;
        [SerializeField, Range(0f, 1f)] private float baselineAlpha = 0.6f;
        [SerializeField] private Material baselineMaterial;
        [SerializeField] private ColorValue success;
        [SerializeField] private ColorValue danger;
        [SerializeField] private ColorValue neutral;
        [Tooltip("Unchecked prediction colour: the path is not a result until the ship lands.")]
        [SerializeField] private ColorValue pending;
        [SerializeField, Range(0f, 1f)] private float pendingAlpha = 0.55f;

        private TrajectoryEndMarker marker;
        private Coroutine pulse;
        private Vector3 markerBaseScale;
        private bool hasMarkerScale;
        private Material lineMaterial;

        private readonly struct LineStyle
        {
            public readonly Color Line;
            public readonly Color Marker;
            public readonly bool Pending;

            public LineStyle(Color line, Color marker, bool pending)
            {
                Line = line;
                Marker = marker;
                Pending = pending;
            }
        }

        private void Awake()
        {
            CaptureMarkerScale();
            lineMaterial = line.sharedMaterial;
            marker = new TrajectoryEndMarker(
                endMarker, endMarkerRenderer, success, danger, neutral);
        }

        public void Show(
            TrajectoryResult result, Color routeColor, Vector3 fieldCentre, float fieldHalf, bool pending)
        {
            StopPulse();
            int count = result.Points != null ? result.Points.Count : 0;
            if (count < 2)
            {
                Hide();
                return;
            }

            line.sharedMaterial = lineMaterial;
            Color lineColor = pending ? routeColor.WithAlpha(pendingAlpha) : routeColor;
            Color markerColor = pending ? lineColor : marker.ColorFor(result.Outcome);
            ShowResult(result, new LineStyle(lineColor, markerColor, pending), fieldCentre, fieldHalf);
        }

        private void ShowResult(
            TrajectoryResult result, LineStyle style, Vector3 fieldCentre, float fieldHalf)
        {
            FillLine(result.Points);
            SetColor(style.Line);
            Vector3 end = result.Points[result.Points.Count - 1];
            Vector3 heading = Heading(result.Points);
            bool miss = !style.Pending && result.Outcome == TrajectoryOutcome.Miss;
            if (miss)
            {
                marker.Hide();
                ShowHeading(end, heading, style.Line, DistanceToBox(end, heading, fieldCentre, fieldHalf));
            }
            else
            {
                marker.Show(end, heading, style.Marker);
                HideHeading();
            }

            line.enabled = true;
        }

        public void ShowBaseline(Vector3 start, Vector3 end, Color routeColor)
        {
            StopPulse();
            HideHeading();
            marker.Hide();
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.sharedMaterial = baselineMaterial;
            line.widthMultiplier = baselineWidth;
            line.textureScale = new Vector2(16f, 1f);
            line.SetBaseColor(routeColor.WithAlpha(baselineAlpha));
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

        private void ShowHeading(Vector3 end, Vector3 heading, Color color, float length)
        {
            if (heading.sqrMagnitude < HeadingEpsilon || length <= 0f)
            {
                headingLine.enabled = false;
                return;
            }

            headingLine.positionCount = 2;
            headingLine.SetPosition(0, end);
            headingLine.SetPosition(1, end + heading * length);
            headingLine.SetBaseColor(color);
            headingLine.startColor = color;
            headingLine.endColor = color;
            headingLine.enabled = true;
        }

        // The heading reaches the field's edge: from a point inside the frame cube, along a direction,
        // out to the boundary.
        private static float DistanceToBox(Vector3 origin, Vector3 direction, Vector3 centre, float half)
        {
            float far = float.MaxValue;
            for (int axis = 0; axis < 3; axis++)
            {
                float component = direction[axis];
                if (Mathf.Abs(component) < 1e-6f)
                    continue;

                float low = (centre[axis] - half - origin[axis]) / component;
                float high = (centre[axis] + half - origin[axis]) / component;
                far = Mathf.Min(far, Mathf.Max(low, high));
            }

            return far == float.MaxValue || far < 0f ? 0f : far;
        }

        // The overall run direction (start to end), so a trajectory too short for a last-two-point
        // delta — a first-step crash — still has a heading.
        private static Vector3 Heading(IReadOnlyList<Vector3> points)
        {
            Vector3 delta = points[points.Count - 1] - points[0];
            return delta.sqrMagnitude < HeadingEpsilon ? Vector3.zero : delta.normalized;
        }

        public void Hide()
        {
            StopPulse();
            line.positionCount = 0;
            line.enabled = false;
            HideHeading();
            marker.Hide();
        }

        private void HideHeading()
        {
            headingLine.positionCount = 0;
            headingLine.enabled = false;
        }

        public void Pulse()
        {
            if (!marker.IsActive)
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
                float bump = 1f + Mathf.Sin(Mathf.Clamp01(t / PulseSecs) * Mathf.PI) * 1.4f;
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

        private void SetColor(Color color)
        {
            line.SetBaseColor(color);
            line.startColor = Dim(color);
            line.endColor = color;
        }

        private static Color Dim(Color color) =>
            new Color(color.r * TailDim, color.g * TailDim, color.b * TailDim, color.a);
    }
}
