using DigitalLove.Global;
using DigitalLove.VFX;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class PlanetBehaviour : MonoBehaviour
    {
        [SerializeField] private Outline outline;
        [SerializeField] private Transform body;
        [SerializeField] private ScalePunch scalePunch;
        [SerializeField] private float maxOutlineWidth = 5f;
        [SerializeField] private ColorValue routeColor;
        [SerializeField] private ColorValue defaultColor;
        [SerializeField] private Renderer rend;

        private float percentage;

        public bool IsDestination => percentage >= 1;
        public float RadiusOffset => body.lossyScale.x;

        public void SetIsDestination(float percentage)
        {
            if (percentage >= 1 && !IsDestination)
                scalePunch.Animate();
            this.percentage = percentage;
        }

        private void Update()
        {
            outline.enabled = percentage > 0 && !IsDestination;
            outline.OutlineWidth = (1 - percentage) * maxOutlineWidth;
        }

        public void Setup(float radius)
        {
            body.localScale = Vector3.one * radius;
        }

        public void SetIsInRoute(bool isInRoute)
        {
            rend.material.color = isInRoute ? routeColor.value : defaultColor.value;
        }
    }
}