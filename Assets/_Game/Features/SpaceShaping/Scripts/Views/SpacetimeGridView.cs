using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpacetimeGridView : MonoBehaviour
    {
        [SerializeField] private LineRenderer[] lines;
        [SerializeField] private Vector3 gridMin = new(-6f, -4f, -3f);
        [SerializeField] private Vector3 gridMax = new(6f, 4f, 3f);
        [SerializeField] private int gridCount = 4;
        [SerializeField] private float depthScale = 1.5f;
        [SerializeField] private float maxDisplacement = 0.8f;

        public void Render(SpacetimeField field)
        {
            int n = Mathf.Max(2, gridCount);
            int perAxis = n * n;
            for (int index = 0; index < lines.Length; index++)
                RenderLine(lines[index], index / perAxis, index % perAxis, n, field);
        }

        private void RenderLine(LineRenderer line, int axis, int rem, int n, SpacetimeField field)
        {
            int i = rem / n;
            int j = rem % n;
            line.positionCount = n;
            for (int s = 0; s < n; s++)
            {
                Vector3 node = LatticeGrid.Node(axis, s, i, j, n, gridMin, gridMax);
                line.SetPosition(s, LatticeGrid.Warp(node, field, depthScale, maxDisplacement));
            }
        }
    }
}
