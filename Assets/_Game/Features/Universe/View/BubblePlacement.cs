using UnityEngine;

namespace DigitalLove.Game.Universe
{
    public readonly struct BubblePlacement
    {
        public readonly Vector3 OffsetLocal;
        public readonly Quaternion Yaw;

        public BubblePlacement(Vector3 offsetLocal, Quaternion yaw)
        {
            OffsetLocal = offsetLocal;
            Yaw = yaw;
        }

        public static BubblePlacement FromAnchor(
            Vector3 anchorPosition,
            Vector3 homePosition,
            Quaternion homeRotation,
            float scale)
        {
            Vector3 worldOffset = anchorPosition - homePosition;
            worldOffset.y = 0f;
            return new BubblePlacement(
                Quaternion.Inverse(homeRotation) * worldOffset / Mathf.Max(scale, 0.0001f),
                homeRotation);
        }
    }
}
