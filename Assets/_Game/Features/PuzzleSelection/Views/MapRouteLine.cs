using System;
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
        private readonly Action<RunSlotView> storeFieldPoint;

        public MapRouteLine(
            LineRenderer[] segments, ColorValue source, float alpha, int count)
        {
            this.segments = segments;
            points = new Vector3[count];
            color = source.value;
            this.alpha = Mathf.Clamp01(alpha);
            storeFieldPoint = StoreFieldPoint;
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i].useWorldSpace = false;
                segments[i].loop = false;
                segments[i].positionCount = 2;
            }
        }

        public void Update(
            PuzzleMap map, RunSlotView selected,
            ViewPool<RunSlotView> fields, Vector3 cullFocus, float near, float far)
        {
            if (points.Length == 0)
            {
                HideAll();
                return;
            }

            UpdatePoints(map, selected, fields);
            for (int i = 0; i < segments.Length; i++)
                DrawSegment(i, cullFocus, near, far);
        }

        private void UpdatePoints(
            PuzzleMap map, RunSlotView selected,
            ViewPool<RunSlotView> fields)
        {
            points[map.SelectedIndex] = selected.SunMapPosition;
            fields.ForEachFrom(0, storeFieldPoint);
        }

        private void StoreFieldPoint(RunSlotView field)
        {
            if (field.MapIndex >= 0 && field.MapIndex < points.Length)
                points[field.MapIndex] = field.SunMapPosition;
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
