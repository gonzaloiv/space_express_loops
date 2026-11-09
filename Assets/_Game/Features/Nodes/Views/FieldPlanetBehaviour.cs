using System.Collections;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class FieldPlanetBehaviour : MonoBehaviour
    {
        [SerializeField] private NodeBody nodeBody;
        [SerializeField] private PlanetNameLabel nameLabel;

        private bool isVisible = true;
        private bool labelsVisible = true;
        private bool labelOnly;
        private bool hasVariant;
        private float radius = 0.1f;
        private int variantSeed;
        private string currentLabel = string.Empty;
        private Coroutine pulse;

        public bool IsVisible => isVisible;
        public float Radius => radius;

        // A short swell of the body and its rim: the destination's arrival on a solve.
        public void PlayArrival(float secs)
        {
            StopArrival();
            pulse = StartCoroutine(PulseArrival(Mathf.Max(secs, 0.0001f)));
        }

        private void StopArrival()
        {
            if (pulse != null)
            {
                StopCoroutine(pulse);
                pulse = null;
            }

            nodeBody.SetPulse(0f);
        }

        private IEnumerator PulseArrival(float secs)
        {
            float elapsed = 0f;
            while (elapsed < secs)
            {
                nodeBody.SetPulse(Mathf.Sin(Mathf.PI * Mathf.Clamp01(elapsed / secs)));
                elapsed += Time.deltaTime;
                yield return null;
            }

            nodeBody.SetPulse(0f);
            pulse = null;
        }

        private void OnDisable() => StopArrival();

        public void SetLabelsVisible(bool visible)
        {
            labelsVisible = visible;
            if (!visible)
                nameLabel.Hide();
        }

        public void SetLabel(string text)
        {
            currentLabel = text;
            nameLabel.Place(text);
        }

        public void SetLabelColor(Color color) => nameLabel.SetColor(color);

        public void ShowLabelOnly(string text)
        {
            gameObject.SetActive(true);
            labelOnly = true;
            SetVisible(false);
            labelsVisible = true;
            currentLabel = text;
            nameLabel.Place(text);
        }

        public void SetCulled(bool culled)
        {
            SetVisible(!culled && !labelOnly);
            if (culled)
                nameLabel.Hide();
            else if (labelsVisible)
                nameLabel.Place(currentLabel);
        }

        public void SetFade(float amount)
        {
            nameLabel.SetFade(amount);
            if (labelOnly)
                return;

            if (amount <= 0.001f)
            {
                nodeBody.ClearTint();
                return;
            }

            Color gray = Color.Lerp(Color.white, new Color(0.4f, 0.4f, 0.4f, 1f), amount);
            nodeBody.SetTint(gray);
        }

        public void Initialize()
        {
            StopArrival();
            radius = 0.1f;
            hasVariant = false;
            labelOnly = false;
            currentLabel = string.Empty;
            nodeBody.Init(radius, 0);
            isVisible = false;
            nodeBody.SetVisible(false);
            nameLabel.Hide();
        }

        public void Spawn(Vector3 localPosition, float spawnRadius, int seed, string name)
        {
            StopArrival();
            radius = spawnRadius;
            labelOnly = false;
            transform.localPosition = localPosition;

            if (hasVariant && variantSeed == seed)
                nodeBody.SetRadius(spawnRadius);
            else
                SpawnWithVariant(spawnRadius, seed);

            gameObject.SetActive(true);
            SetVisible(true);
            currentLabel = name;
            if (labelsVisible)
                nameLabel.Place(name);
            else
                nameLabel.Hide();
        }

        public void Hide()
        {
            StopArrival();
            labelOnly = false;
            SetVisible(false);
            nameLabel.Hide();
            gameObject.SetActive(false);
        }

        public void SetRouteColor(Color color) => nodeBody.SetRouteColor(color);

        private void SetVisible(bool visible)
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
