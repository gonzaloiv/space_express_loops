using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public static class LatticeGrid
    {
        public static Vector3 Node(int axis, int s, int i, int j, int n, Vector3 min, Vector3 max)
        {
            float a = T(s, n);
            float b = T(i, n);
            float c = T(j, n);
            if (axis == 0)
                return new Vector3(Lerp(min.x, max.x, a), Lerp(min.y, max.y, b), Lerp(min.z, max.z, c));
            if (axis == 1)
                return new Vector3(Lerp(min.x, max.x, b), Lerp(min.y, max.y, a), Lerp(min.z, max.z, c));
            return new Vector3(Lerp(min.x, max.x, b), Lerp(min.y, max.y, c), Lerp(min.z, max.z, a));
        }

        public static Vector3 Warp(Vector3 node, SpacetimeField field, float scale, float maxDisplacement)
        {
            Vector3 pull = field.Pull(node) * scale;
            if (pull.sqrMagnitude > maxDisplacement * maxDisplacement)
                pull = pull.normalized * maxDisplacement;
            return node + pull;
        }

        private static float T(int k, int n) => n <= 1 ? 0.5f : (float)k / (n - 1);

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}
