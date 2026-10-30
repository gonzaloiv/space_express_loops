using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class FieldPlanetBehaviour : MonoBehaviour, IPoolableNode
    {
        [SerializeField] private NodeBody nodeBody;
        [SerializeField] private PlanetNameLabel nameLabel;

        private bool isVisible = true;
        private bool hasVariant;
        private float radius = 0.1f;
        private int variantSeed;

        public bool IsActive => gameObject.activeInHierarchy;
        public Vector3 LocalPosition => transform.localPosition;
        public Vector3 Position => nodeBody.Position;
        public float Radius => radius;

        public void Initialize()
        {
            radius = 0.1f;
            hasVariant = false;
            nodeBody.Init(radius, 0);
            isVisible = false;
            nodeBody.SetVisible(false);
            nameLabel.Hide();
        }

        public void Spawn(Vector3 localPosition, float spawnRadius, int seed, string name)
        {
            radius = spawnRadius;
            transform.localPosition = localPosition;

            if (hasVariant && variantSeed == seed)
                nodeBody.SetRadius(spawnRadius);
            else
                SpawnWithVariant(spawnRadius, seed);

            gameObject.SetActive(true);
            SetVisible(true);
            nameLabel.Place(name);
        }

        public void Hide()
        {
            SetVisible(false);
            nameLabel.Hide();
            gameObject.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            if (isVisible == visible)
                return;

            isVisible = visible;
            nodeBody.SetVisible(visible);
        }

        private void SpawnWithVariant(float spawnRadius, int seed)
        {
            hasVariant = true;
            variantSeed = seed;
            nodeBody.Init(spawnRadius, seed);
        }
    }
}
