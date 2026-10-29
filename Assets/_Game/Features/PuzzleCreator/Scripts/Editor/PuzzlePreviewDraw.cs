using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    internal static class PuzzlePreviewDraw
    {
        private const float Margin = 1.15f;
        private const float HeadingWidth = 1.2f;
        private const float DashLength = 5f;
        private const float GapLength = 4f;
        private const float ArrowSize = 8f;
        private const float ArrowDeg = 24f;

        private static readonly Color Back = new(0.12f, 0.12f, 0.14f, 1f);
        private static readonly Color Source = new(0.78f, 0.78f, 0.72f, 1f);
        private static readonly Color StartPath = new(0.45f, 0.55f, 0.78f, 1f);
        private static readonly Color SolvedPath = new(0.35f, 1f, 0.5f, 1f);
        private static readonly Color Endpoint = new(1f, 0.8f, 0.3f, 1f);
        private static readonly Color ZoneLine = new(0.6f, 0.85f, 1f, 0.9f);
        private static readonly Color Height = new(0.35f, 0.35f, 0.4f, 0.9f);
        private static readonly Color HeadingPath = new(0.35f, 1f, 0.5f, 0.7f);

        public static void Draw(Rect rect, PuzzleData puzzle, PuzzleVerification run)
        {
            EditorGUI.DrawRect(rect, Back);
            float scale = Scale(rect, puzzle);
            Vector2 centre = rect.center;

            DrawZone(run.Solution, scale, centre);
            DrawSources(puzzle, scale, centre);
            DrawPath(run.Start, scale, centre, StartPath);
            DrawPath(run.SolvedRun, scale, centre, SolvedPath);
            DrawHeading(rect, run.SolvedRun, scale, centre);
            Disc(puzzle.Origin, scale, centre, Endpoint, 4f);
            Disc(puzzle.Destination, scale, centre, Endpoint, 4f);
        }

        private static float Scale(Rect rect, PuzzleData puzzle)
        {
            float extent = Mathf.Max(FieldExtent(puzzle), 0.01f);
            return Mathf.Min(rect.width, rect.height) * 0.5f / (extent * Margin);
        }

        private static float FieldExtent(PuzzleData puzzle)
        {
            float extent = puzzle.Distance * 0.5f;
            if (puzzle.sources == null)
                return extent;

            for (int i = 0; i < puzzle.sources.Length; i++)
            {
                PuzzleSource source = puzzle.sources[i];
                extent = Mathf.Max(extent, source.home.magnitude + source.scale * 0.5f);
            }

            return extent;
        }

        private static Vector2 ToGui(Vector3 world, float scale, Vector2 centre) =>
            centre + new Vector2(world.x, -world.z) * scale;

        private static void Disc(Vector3 world, float scale, Vector2 centre, Color color, float radius)
        {
            Vector2 point = ToGui(world, scale, centre);
            Handles.color = color;
            Handles.DrawSolidDisc(point, Vector3.forward, radius);

            if (Mathf.Abs(world.y) < 0.0001f)
                return;

            Handles.color = Height;
            Handles.DrawAAPolyLine(1.5f, point, point + new Vector2(0f, -world.y * scale));
        }

        private static void DrawSources(PuzzleData puzzle, float scale, Vector2 centre)
        {
            if (puzzle.sources == null)
                return;

            for (int i = 0; i < puzzle.sources.Length; i++)
                Disc(puzzle.sources[i].home, scale, centre, Source, puzzle.sources[i].scale * 0.5f * scale);
        }

        private static void DrawPath(
            TrajectoryResult run, float scale, Vector2 centre, Color color)
        {
            if (run.Points == null || run.Points.Count < 2)
                return;

            Handles.color = color;
            for (int i = 1; i < run.Points.Count; i++)
                Handles.DrawLine(ToGui(run.Points[i - 1], scale, centre), ToGui(run.Points[i], scale, centre));
        }

        private static void DrawHeading(Rect rect, TrajectoryResult run, float scale, Vector2 centre)
        {
            if (run.Points == null || run.Points.Count < 2)
                return;

            int last = run.Points.Count - 1;
            Vector2 end = ToGui(run.Points[last], scale, centre);
            Vector2 delta = end - ToGui(run.Points[last - 1], scale, centre);
            if (delta.sqrMagnitude < 0.000001f)
                return;

            Vector2 dir = delta.normalized;
            Handles.color = HeadingPath;
            DrawDashed(end, end + dir * (rect.width + rect.height), DashLength, GapLength);
            DrawArrowHead(end, dir, ArrowSize);
        }

        private static void DrawDashed(Vector2 from, Vector2 to, float dash, float gap)
        {
            Vector2 delta = to - from;
            float length = delta.magnitude;
            if (length < 0.001f)
                return;

            Vector2 dir = delta / length;
            float step = Mathf.Max(dash + gap, 0.01f);
            for (float t = 0f; t < length; t += step)
            {
                Vector2 a = from + dir * t;
                Vector2 b = from + dir * Mathf.Min(t + dash, length);
                Handles.DrawAAPolyLine(HeadingWidth, a, b);
            }
        }

        private static void DrawArrowHead(Vector2 tip, Vector2 dir, float size)
        {
            Vector2 back = -dir;
            Handles.DrawAAPolyLine(HeadingWidth, tip, tip + Rotate(back, ArrowDeg) * size);
            Handles.DrawAAPolyLine(HeadingWidth, tip, tip + Rotate(back, -ArrowDeg) * size);
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        private static void DrawZone(PuzzleZone[] solution, float scale, Vector2 centre)
        {
            if (solution == null)
                return;

            Handles.color = ZoneLine;
            for (int i = 0; i < solution.Length; i++)
            {
                PuzzleZone zone = solution[i];
                Vector2 point = ToGui(zone.centre, scale, centre);
                Vector2 half = new(zone.halfExtents.x * scale, zone.halfExtents.z * scale);
                Handles.DrawAAPolyLine(
                    1.5f,
                    (Vector3)(point + new Vector2(-half.x, -half.y)),
                    (Vector3)(point + new Vector2(half.x, -half.y)),
                    (Vector3)(point + new Vector2(half.x, half.y)),
                    (Vector3)(point + new Vector2(-half.x, half.y)),
                    (Vector3)(point + new Vector2(-half.x, -half.y)));
            }
        }
    }
}
