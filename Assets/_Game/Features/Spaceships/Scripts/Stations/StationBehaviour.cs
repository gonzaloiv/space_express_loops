using DigitalLove.Game.Planets;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class StationBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform spawnAnchor;
        [SerializeField] private float surfaceOffsetMultiplier = 1.25f;

        private PlanetBehaviour anchoredPlanet;

        public bool IsActive => gameObject.activeInHierarchy;
        public PlanetBehaviour AnchoredPlanet => anchoredPlanet;
        public Pose SpawnPose => (spawnAnchor != null ? spawnAnchor : transform).ToWorldPose();

        public void RepositionAt(PlanetBehaviour planet)
        {
            anchoredPlanet = planet;

            float radius = planet.PlanetBody.Radius;
            Vector3 outward = planet.transform.up;

            transform.position = planet.Position + outward * radius * surfaceOffsetMultiplier;
            transform.rotation = Quaternion.LookRotation(outward, Vector3.up);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            anchoredPlanet = null;
            gameObject.SetActive(false);
        }
    }
}
