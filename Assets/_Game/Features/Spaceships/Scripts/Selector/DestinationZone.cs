using UnityEngine;
using DigitalLove.Game.Planets;

namespace DigitalLove.Game.Spaceships
{
    public class DestinationZone : MonoBehaviour
    {
        [SerializeField] private Renderer zoneRenderer;

        private Color insideColor;

        public void SetColor(Color color)
        {
            insideColor = color;
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void DoUpdate(float countdown, float secsToSelect, Vector3 originPosition, PlanetBehaviour destinationPlanet)
        {
            transform.position = destinationPlanet.Position;

            float t = Mathf.Clamp01(1f - (countdown / secsToSelect));
            float fromScale = 1f;
            float toScale = 2f;
            float currentScale = Mathf.Lerp(fromScale, toScale, t);
            transform.localScale = Vector3.one * currentScale;

            float zoneRadius = destinationPlanet.PlanetBody.Radius * 3f;
            Debug.LogWarning($"zoneRadius: {zoneRadius}");
            float distanceToZoneCenter = Vector3.Distance(originPosition, transform.position);
            Debug.LogWarning($"distanceToZoneCenter: {distanceToZoneCenter}");
            zoneRenderer.material.color = distanceToZoneCenter <= zoneRadius ? insideColor : Color.white;
        }
    }
}