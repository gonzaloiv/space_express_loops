using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceZoneView : MonoBehaviour
    {
        [SerializeField] private Vector3 baseSize = new(8f, 6f, 4f);
        [SerializeField] private float falloff = 0.5f;

        public SpaceZone Zone
        {
            get
            {
                Vector3 size = transform.localScale;
                float scale = baseSize.x > 0f ? size.x / baseSize.x : 1f;
                return new SpaceZone(transform.localPosition, size * 0.5f, scale, falloff);
            }
        }
    }
}
