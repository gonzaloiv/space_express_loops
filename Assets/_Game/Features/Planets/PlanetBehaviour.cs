using DigitalLove.VFX;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class PlanetBehaviour : MonoBehaviour
    {
        [SerializeField] private Outline outline;
        [SerializeField] private Transform body;
        [SerializeField] private ScalePunch scalePunch;

        private bool isDestination;

        public bool IsDestination => isDestination;
        public float RadiusOffset => body.lossyScale.x;

        public void SetIsDestination(bool isDestination)
        {
            if (isDestination && !this.isDestination)
                scalePunch.Animate();
            this.isDestination = isDestination;
        }

        private void Update()
        {
            outline.enabled = isDestination;
        }

        public void Setup(float radius)
        {
            body.localScale = Vector3.one * radius;
        }
    }
}