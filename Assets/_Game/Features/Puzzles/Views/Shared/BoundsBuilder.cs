using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public struct BoundsBuilder
    {
        private Bounds bounds;
        private bool any;

        public bool HasBounds => any;
        public Vector3 Centre => bounds.center;
        public float Extent => Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z)) * 0.5f;

        public void AddPoint(Vector3 point)
        {
            if (!any)
            {
                bounds = new Bounds(point, Vector3.zero);
                any = true;
                return;
            }

            bounds.Encapsulate(point);
        }

        public void AddSphere(Vector3 centre, float radius)
        {
            AddPoint(centre);
            bounds.Encapsulate(centre + Vector3.one * radius);
            bounds.Encapsulate(centre - Vector3.one * radius);
        }

        public bool TryGet(out Vector3 centre, out float extent)
        {
            centre = Centre;
            extent = Extent;
            return any;
        }
    }
}
