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

        public void Tick(
            PuzzleMap map, RunSlotView selected,
            ViewPool<RunSlotView> fields, MapFade fade)
        {
            if (points.Length == 0)
            {
                HideAll();
                return;
            }

            UpdatePoints(map, selected, fields);
            int end = EndIndex(map);
            for (int i = 0; i < segments.Length; i++)
                DrawSegment(i, end, fade);
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

        private void DrawSegment(int index, int end, MapFade fade)
        {
            LineRenderer segment = segments[index];
            if (index >= end || index + 1 >= points.Length)
            {
                segment.enabled = false;
                return;
            }

            Vector3 start = points[index];
            Vector3 endPoint = points[index + 1];
            float distance = Vector3.Distance((start + endPoint) * 0.5f, fade.Focus);
            segment.enabled = distance < fade.Far;
            if (!segment.enabled)
                return;

            Color tint = color;
            tint.a = alpha * (1f - Mathf.InverseLerp(fade.Near, fade.Far, distance));
            segment.startColor = tint;
            segment.endColor = tint;
            segment.SetPosition(0, start);
            segment.SetPosition(1, endPoint);
        }

        // The route is the open chain up to the job that is next, not a ring around the map.
        private static int EndIndex(PuzzleMap map)
        {
            for (int i = 0; i < map.Count; i++)
            {
                if (map[i].State == JobState.Current)
                    return i;
            }

            int lastSolved = -1;
            for (int i = 0; i < map.Count; i++)
            {
                if (map[i].State == JobState.Solved)
                    lastSolved = i;
            }

            return lastSolved >= 0 ? lastSolved : Mathf.Max(map.SelectedIndex, 0);
        }

        private void HideAll()
        {
            for (int i = 0; i < segments.Length; i++)
                segments[i].enabled = false;
        }
    }

    internal readonly struct MapFade
    {
        public readonly Vector3 Focus;
        public readonly float Near;
        public readonly float Far;

        public MapFade(Vector3 focus, float near, float far)
        {
            Focus = focus;
            Near = near;
            Far = far;
        }
    }
}
