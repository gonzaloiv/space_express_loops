using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceZoneView : MonoBehaviour
    {
        [SerializeField] private float falloff = 0.5f;

        private float scale = 1f;

        public SpaceZone Zone => new(Centre, Size * 0.5f, scale, falloff);

        public Vector3 Centre => transform.localPosition;

        public Vector3 Size => transform.localScale;

        public float Scale => scale;

        public void SetFrame(Vector3 centre, Vector3 size)
        {
            transform.localPosition = centre;
            transform.localScale = size;
        }

        public void SetScale(float value) => scale = Mathf.Max(value, 0.01f);
    }
}
