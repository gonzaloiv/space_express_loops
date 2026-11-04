using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public readonly struct PuzzleMapLayout
    {
        private const float GoldenAngle = 2.39996323f;

        private readonly Vector3[] positions;

        private PuzzleMapLayout(Vector3[] positions)
        {
            this.positions = positions;
        }

        public int Count => positions != null ? positions.Length : 0;

        public Vector3 At(int index) =>
            positions != null && index >= 0 && index < positions.Length
                ? positions[index]
                : Vector3.zero;

        public static PuzzleMapLayout Galaxy(int count, float radius = 0.5f, float height = 0.15f)
        {
            if (count <= 0)
                return new PuzzleMapLayout(System.Array.Empty<Vector3>());

            Vector3[] result = new Vector3[count];
            float safeRadius = Mathf.Max(radius, 0.01f);
            float safeHeight = Mathf.Max(height, 0f);
            for (int i = 0; i < count; i++)
            {
                float t = count <= 1 ? 0f : (i + 0.5f) / count;
                float r = safeRadius * Mathf.Sqrt(t);
                float angle = i * GoldenAngle;
                float x = Mathf.Cos(angle) * r;
                float z = Mathf.Sin(angle) * r;
                float y = Mathf.Sin(angle * 2f) * safeHeight * (1f - t);
                result[i] = new Vector3(x, y, z);
            }

            return new PuzzleMapLayout(result);
        }
    }
}
