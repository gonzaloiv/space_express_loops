using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class RoomLocalYRangeCache
    {
        private bool isValid;
        private Bounds cachedBounds;
        private Vector3 cachedPosition;
        private Quaternion cachedRotation;
        private float cachedMinY;
        private float cachedMaxY;

        public void Invalidate() => isValid = false;

        public bool TryGet(MRUKRoom room, Transform localSpace, out float minY, out float maxY)
        {
            Bounds bounds = room.GetRoomBounds();
            if (bounds.size.sqrMagnitude <= 0f)
            {
                Invalidate();
                minY = maxY = 0f;
                return false;
            }

            if (isValid
                && cachedBounds == bounds
                && cachedPosition == localSpace.position
                && cachedRotation == localSpace.rotation)
            {
                minY = cachedMinY;
                maxY = cachedMaxY;
                return true;
            }

            Compute(bounds, localSpace, out minY, out maxY);
            cachedMinY = minY;
            cachedMaxY = maxY;
            cachedBounds = bounds;
            cachedPosition = localSpace.position;
            cachedRotation = localSpace.rotation;
            isValid = true;
            return true;
        }

        private static void Compute(Bounds bounds, Transform localSpace, out float minY, out float maxY)
        {
            minY = float.PositiveInfinity;
            maxY = float.NegativeInfinity;
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        float localY = localSpace.InverseTransformPoint(
                            center + Vector3.Scale(extents, new Vector3(x, y, z))).y;
                        minY = Mathf.Min(minY, localY);
                        maxY = Mathf.Max(maxY, localY);
                    }
                }
            }
        }
    }
}
