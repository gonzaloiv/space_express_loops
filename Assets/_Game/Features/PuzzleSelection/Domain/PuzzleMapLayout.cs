using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public readonly struct PuzzleMapLayout
    {
        private const float GoldenAngle = 2.39996323f;

        private readonly Vector3[] positions;
        private readonly Vector3[] centres;
        private readonly float[] radii;

        private PuzzleMapLayout(Vector3[] positions, Vector3[] centres, float[] radii)
        {
            this.positions = positions;
            this.centres = centres;
            this.radii = radii;
        }

        public int Count => positions != null ? positions.Length : 0;

        public int SystemCount => centres != null ? centres.Length : 0;

        public Vector3 At(int index) =>
            positions != null && index >= 0 && index < positions.Length
                ? positions[index]
                : Vector3.zero;

        public Vector3 Centre(int system) =>
            centres != null && system >= 0 && system < centres.Length ? centres[system] : Vector3.zero;

        public float Radius(int system) =>
            radii != null && system >= 0 && system < radii.Length ? radii[system] : 0f;

        public static PuzzleMapLayout Galaxy(int count, float radius = 0.24f, float height = 0f)
        {
            if (count <= 0)
                return new PuzzleMapLayout(
                    System.Array.Empty<Vector3>(), System.Array.Empty<Vector3>(), System.Array.Empty<float>());

            Vector3[] result = new Vector3[count];
            Spiral(result, 0, count, Vector3.zero, radius, height);
            return new PuzzleMapLayout(result, new[] { Vector3.zero }, new[] { Mathf.Max(radius, 0.01f) });
        }

        // One small spiral per chapter, side by side with a gap between them, centred on the map. Each
        // spiral keeps the galaxy's density (its area grows with its job count), so the dioramas sit
        // as far apart as they always did; one chapter is the plain galaxy.
        public static PuzzleMapLayout Systems(
            IReadOnlyList<int> sizes, float radius = 0.24f, float gap = 0.1f)
        {
            int total = Total(sizes);
            if (sizes == null || sizes.Count <= 1 || total <= 0)
                return Galaxy(total, radius);

            Vector3[] centres = new Vector3[sizes.Count];
            float[] radii = new float[sizes.Count];
            PlaceCentres(sizes, total, Mathf.Max(radius, 0.01f), Mathf.Max(gap, 0f), centres, radii);

            Vector3[] result = new Vector3[total];
            int start = 0;
            for (int i = 0; i < sizes.Count; i++)
            {
                Spiral(result, start, sizes[i], centres[i], radii[i], 0f);
                start += Mathf.Max(sizes[i], 0);
            }

            return new PuzzleMapLayout(result, centres, radii);
        }

        private static void PlaceCentres(
            IReadOnlyList<int> sizes, int total, float radius, float gap, Vector3[] centres, float[] radii)
        {
            float x = 0f;
            for (int i = 0; i < sizes.Count; i++)
            {
                radii[i] = radius * Mathf.Sqrt(Mathf.Max(sizes[i], 1) / (float)total);
                if (i > 0)
                    x += radii[i - 1] + gap + radii[i];
                centres[i] = new Vector3(x, 0f, 0f);
            }

            float middle = (centres[0].x - radii[0] + x + radii[sizes.Count - 1]) * 0.5f;
            for (int i = 0; i < centres.Length; i++)
                centres[i].x -= middle;
        }

        private static void Spiral(
            Vector3[] result, int start, int count, Vector3 centre, float radius, float height)
        {
            float safeRadius = Mathf.Max(radius, 0.01f);
            float safeHeight = Mathf.Max(height, 0f);
            for (int i = 0; i < count; i++)
            {
                float t = count <= 1 ? 0f : (i + 0.5f) / count;
                float r = safeRadius * Mathf.Sqrt(t);
                float angle = i * GoldenAngle;
                float y = Mathf.Sin(angle * 2f) * safeHeight * (1f - t);
                result[start + i] = centre + new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r);
            }
        }

        private static int Total(IReadOnlyList<int> sizes)
        {
            int total = 0;
            if (sizes == null)
                return total;

            for (int i = 0; i < sizes.Count; i++)
                total += Mathf.Max(sizes[i], 0);
            return total;
        }
    }
}
