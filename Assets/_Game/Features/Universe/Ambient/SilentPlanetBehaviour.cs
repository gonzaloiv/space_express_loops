using DigitalLove.Game.Nodes;
using UnityEngine;

namespace DigitalLove.Game.SilentPlanets
{
    public class SilentPlanetBehaviour : MonoBehaviour, IPoolableNode
    {
        [SerializeField] private NodeBody nodeBody;

        private bool isVisible = true;
        private float radius = 0.1f;

        public bool IsActive => gameObject.activeInHierarchy;
        public Vector3 LocalPosition => transform.localPosition;
        public Vector3 Position => nodeBody.Position;
        public float Radius => radius;

        public void Initialize()
        {
            radius = 0.1f;
            nodeBody.Init(radius, 0);
            isVisible = false;
            nodeBody.SetVisible(false);
        }

        public void Spawn(Vector3 localPosition, float spawnRadius, int variantSeed)
        {
            radius = spawnRadius;
            transform.localPosition = localPosition;
            nodeBody.Init(radius, variantSeed);
            gameObject.SetActive(true);
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
            gameObject.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            if (isVisible == visible)
                return;

            isVisible = visible;
            nodeBody.SetVisible(visible);
        }
    }
}
