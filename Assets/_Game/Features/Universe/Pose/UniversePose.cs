using UnityEngine;

namespace DigitalLove.Game.Universe
{
    internal readonly struct UniversePose
    {
        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly float scale;

        public UniversePose(Vector3 position, Quaternion rotation, float scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public static UniversePose FromTransform(Transform transform) =>
            new(transform.position, transform.rotation, transform.localScale.x);
    }
}
