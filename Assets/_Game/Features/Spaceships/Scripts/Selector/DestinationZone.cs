using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class DestinationZone : MonoBehaviour
    {
        [SerializeField] private Renderer zoneRenderer;
        [SerializeField] private float destinationZoneRadius = 0.5f;

        private Color insideColor;

        public void SetColor(Color color)
        {
            insideColor = color;
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void DoUpdate(float countdown, float secsToSelect, Vector3 destinationPosition)
        {
            transform.position = destinationPosition;

            float t = Mathf.Clamp01(1f - (countdown / secsToSelect));
            float fromScale = 1f;
            float toScale = 2f;
            float currentScale = Mathf.Lerp(fromScale, toScale, t);
            transform.localScale = Vector3.one * currentScale;

            float zoneRadius = transform.localScale.x * 0.5f * destinationZoneRadius;
            float distanceToZoneCenter = Vector3.Distance(transform.position, transform.position);
            zoneRenderer.material.color = distanceToZoneCenter <= zoneRadius ? insideColor : Color.white;
        }
    }
}