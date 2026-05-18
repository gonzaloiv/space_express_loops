using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class HubBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform spawnAnchor;
        [SerializeField] private PlanetBody planetBody;

        private string id;

        public PlanetBody PlanetBody => planetBody;
        public bool IsActive => gameObject.activeInHierarchy;
        public string Id => id;
        public Vector3 LocalPosition => transform.localPosition;
        public Pose SpawnPose => (spawnAnchor != null ? spawnAnchor : transform).ToWorldPose();

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void SpawnAsHub(string hubId, Vector3 localPosition)
        {
            id = hubId;
            transform.localPosition = localPosition;
            SetActive(true);
            planetBody?.EnsureReadyForRouteColor();
        }

        public void SetRouteColor(Color color)
        {
            planetBody?.SetRouteColor(color);
        }

        public void ResetRouteColor()
        {
            planetBody?.ResetRouteColor();
        }

        public void ResetForPool()
        {
            id = string.Empty;
            ResetRouteColor();
            SetActive(false);
        }
    }
}
