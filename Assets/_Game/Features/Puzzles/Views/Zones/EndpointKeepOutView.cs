using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // One red copy of the cube at a refused move's attempted position.
    public class EndpointKeepOutView : MonoBehaviour
    {
        [SerializeField] private Renderer body;

        private RendererTint tint;

        public void Show(Vector3 centre, Vector3 size, Color danger)
        {
            transform.localPosition = centre;
            transform.localScale = size;
            tint.Set(body, danger);
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
