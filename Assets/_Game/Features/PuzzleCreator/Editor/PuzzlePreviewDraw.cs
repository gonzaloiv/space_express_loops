using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    internal static class PuzzlePreviewDraw
    {
        private const float Margin = 1.15f;

        private static readonly Color Back = new(0.12f, 0.12f, 0.14f, 1f);
        private static readonly Color Source = new(0.78f, 0.78f, 0.72f, 1f);
        private static readonly Color StartPath = new(0.45f, 0.55f, 0.78f, 1f);
        private static readonly Color SolvedPath = new(0.35f, 1f, 0.5f, 1f);
        private static readonly Color Endpoint = new(1f, 0.8f, 0.3f, 1f);
        private static readonly Color ZoneLine = new(0.6f, 0.85f, 1f, 0.9f);

        public static void Draw(Rect rect, PuzzleData puzzle, PuzzleVerification run)
        {
            EditorGUI.DrawRect(rect, Back);
            float scale = Scale(rect, puzzle);
            Vector2 centre = rect.center;

            DrawZone(puzzle, scale, centre);
            DrawSources(puzzle, scale, centre);
            DrawPath(run.Start, scale, centre, StartPath);
            DrawPath(run.SolvedRun, scale, centre, SolvedPath);
            Disc(puzzle.Origin, scale, centre, Endpoint, 4f);
            Disc(puzzle.Destination, scale, centre, Endpoint, 4f);
        }

        private static float Scale(Rect rect, PuzzleData puzzle)
        {
            float extent = Mathf.Max(Mathf.Abs(puzzle.Origin.x), 0.01f);
            return Mathf.Min(rect.width, rect.height) * 0.5f / (extent * Margin);
        }

        private static Vector2 ToGui(Vector3 world, float scale, Vector2 centre) =>
            centre + new Vector2(world.x, -world.z) * scale;

        private static void Disc(Vector3 world, float scale, Vector2 centre, Color color, float radius)
        {
            Handles.color = color;
            Handles.DrawSolidDisc(ToGui(world, scale, centre), Vector3.forward, radius);
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

        private static void DrawZone(PuzzleData puzzle, float scale, Vector2 centre)
        {
            if (puzzle.solution == null)
                return;

            Handles.color = ZoneLine;
            for (int i = 0; i < puzzle.solution.Length; i++)
            {
                PuzzleZone zone = puzzle.solution[i];
                Vector2 point = ToGui(zone.centre, scale, centre);
                Vector2 half = new(zone.size.x * 0.5f * scale, zone.size.z * 0.5f * scale);
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
