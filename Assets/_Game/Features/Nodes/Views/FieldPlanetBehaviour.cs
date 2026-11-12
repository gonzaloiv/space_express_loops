using System.Collections;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class FieldPlanetBehaviour : MonoBehaviour
    {
        [SerializeField] private NodeBody nodeBody;
        [SerializeField] private PlanetNameLabel nameLabel;
        [SerializeField] private ColorValue dimColor;

        private bool isVisible = true;
        private bool labelsVisible = true;
        private bool labelOnly;
        private bool hasVariant;
        private float radius = 0.1f;
        private float fade;
        private float stateDim;
        private int variantSeed;
        private string currentLabel = string.Empty;
        private Coroutine pulse;

        private const float RefusalSecs = 0.24f;

        public bool IsVisible => isVisible;
        public float Radius => radius;

        // The source this planet was spawned for; the endpoint pulse matches on it.
        public int SourceIndex { get; private set; }

        // A short swell of the body and its rim: the destination's arrival on a solve.
        public void PlayArrival(float secs)
        {
            StopArrival();
            pulse = StartCoroutine(PulseArrival(Mathf.Max(secs, 0.0001f)));
        }

        // A short danger swell: a refused move touched this endpoint.
        public void PlayRefusal(Color danger)
        {
            StopArrival();
            pulse = StartCoroutine(PulseRefusal(danger, RefusalSecs));
        }

        private void StopArrival()
        {
            if (pulse != null)
            {
                StopCoroutine(pulse);
                pulse = null;
            }

            nodeBody.SetPulse(0f);
            ApplyTint();
        }

        private IEnumerator PulseRefusal(Color danger, float secs)
        {
            float elapsed = 0f;
            while (elapsed < secs)
            {
                nodeBody.SetTint(danger);
                nodeBody.SetPulse(Mathf.Sin(Mathf.PI * Mathf.Clamp01(elapsed / secs)));
                elapsed += Time.deltaTime;
                yield return null;
            }

            nodeBody.SetPulse(0f);
            ApplyTint();
            pulse = null;
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
            fade = Mathf.Clamp01(amount);
            ApplyTint();
        }

        public void SetStateDim(float amount)
        {
            stateDim = Mathf.Clamp01(amount);
            ApplyTint();
        }

        private void ApplyTint()
        {
            float amount = Mathf.Max(fade, stateDim);
            nameLabel.SetFade(amount);
            ApplyBodyTint(amount, labelOnly);
            OnTintApplied(amount, BodyTint(amount));
        }

        private void ApplyBodyTint(float amount, bool labelOnly)
        {
            if (labelOnly)
                return;

            if (amount <= 0.001f)
            {
                nodeBody.ClearTint();
                return;
            }

            nodeBody.SetTint(BodyTint(amount));
        }

        // The gray a faded or dimmed planet takes; a subclass with extra markers follows it.
        private Color BodyTint(float amount) => Color.Lerp(Color.white, dimColor.value, amount);

        // A subclass with extra markers follows the body's visibility and fade/dim state.
        protected virtual void OnVisibilityChanged(bool visible)
        {
        }

        protected virtual void OnTintApplied(float amount, Color tint)
        {
        }

        public void Initialize()
        {
            StopArrival();
            radius = 0.1f;
            hasVariant = false;
            labelOnly = false;
            fade = 0f;
            stateDim = 0f;
            currentLabel = string.Empty;
            nodeBody.Init(radius, 0);
            isVisible = false;
            nodeBody.SetVisible(false);
            nameLabel.Hide();
        }

        public void Spawn(Vector3 localPosition, float spawnRadius, int seed, string name)
        {
            StopArrival();
            SourceIndex = seed;
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

        public void ClearRouteColor() => nodeBody.ClearRouteColor();

        public void SetMarksShown(bool shown) => nodeBody.SetMarksShown(shown);

        private void SetVisible(bool visible)
        {
            if (isVisible == visible)
                return;

            isVisible = visible;
            nodeBody.SetVisible(visible);
            OnVisibilityChanged(visible);
        }

        private void SpawnWithVariant(float spawnRadius, int seed)
        {
            hasVariant = true;
            variantSeed = seed;
            nodeBody.Init(spawnRadius, seed);
        }
    }
}
