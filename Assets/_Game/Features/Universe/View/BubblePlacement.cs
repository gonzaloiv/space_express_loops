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
    }
}
