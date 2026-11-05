using DigitalLove.Game.SpaceShaping;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    internal class MapRouteLine
    {
        private readonly LineRenderer[] segments;
        private readonly Vector3[] points;
        private readonly Color color;
        private readonly float alpha;

        public MapRouteLine(
            Transform mapRoot, ColorValue source, float alpha, int count)
        {
            segments = mapRoot.GetComponentsInChildren<LineRenderer>(true);
            points = new Vector3[count];
            color = source.value;
            this.alpha = Mathf.Clamp01(alpha);
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i].useWorldSpace = false;
                segments[i].loop = false;
                segments[i].positionCount = 2;
            }
        }

        public void Update(
            PuzzleMap map, RunSlotView selected, Vector3 focus,
            ViewPool<RunSlotView> fields, Vector3 cullFocus, float near, float far)
        {
            if (points.Length == 0)
            {
                HideAll();
                return;
            }

            UpdatePoints(map, selected, focus, fields);
            for (int i = 0; i < segments.Length; i++)
                DrawSegment(i, cullFocus, near, far);
        }

        private void UpdatePoints(
            PuzzleMap map, RunSlotView selected, Vector3 focus,
            ViewPool<RunSlotView> fields)
        {
            points[map.SelectedIndex] = focus + selected.SunLocalPosition;
            fields.ForEachFrom(0, field =>
            {
                if (field.MapIndex >= 0 && field.MapIndex < points.Length)
                {
                    points[field.MapIndex] =
                        field.transform.localPosition + field.SunLocalPosition;
                }
            });
        }

        private void DrawSegment(int index, Vector3 cullFocus, float near, float far)
        {
            LineRenderer segment = segments[index];
            if (index >= points.Length)
            {
                segment.enabled = false;
                return;
            }

            Vector3 start = points[index];
            Vector3 end = points[(index + 1) % points.Length];
            float distance = Vector3.Distance((start + end) * 0.5f, cullFocus);
            segment.enabled = distance < far;
            if (!segment.enabled)
                return;

            Color tint = color;
            tint.a = alpha * (1f - Mathf.InverseLerp(near, far, distance));
            segment.startColor = tint;
            segment.endColor = tint;
            segment.SetPosition(0, start);
            segment.SetPosition(1, end);
        }

        private void HideAll()
        {
            for (int i = 0; i < segments.Length; i++)
                segments[i].enabled = false;
        }
    }
}
