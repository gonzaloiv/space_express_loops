using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class NodeBody : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int RimIntensityId = Shader.PropertyToID("_RimIntensity");

        [SerializeField] private Transform body;
        [SerializeField] private List<Renderer> renderers;
        [Tooltip("How much the arrival pulse swells the body's radius.")]
        [SerializeField] private float pulseSwell = 0.12f;
        [Tooltip("Rim glow added at the peak of the arrival pulse.")]
        [SerializeField] private float pulseRim = 1.6f;

        private Renderer activeRenderer;
        private MaterialPropertyBlock tintBlock;
        private bool visible = true;
        private float radius = 0.1f;
        private Color baseColor = Color.white;
        private Color routeColor = Color.white;
        private Color tintColor = Color.white;
        private bool hasRouteColor;
        private bool hasTint;

        public void Init(float radius, int variantSeed)
        {
            SetRadius(radius);
            SetActiveRendererBySeed(variantSeed);
        }

        public void SetRadius(float value)
        {
            radius = value;
            body.localScale = new Vector3(radius, radius, radius);
        }

        public void SetPulse(float amount)
        {
            float pulse = Mathf.Clamp01(amount);
            body.localScale = new Vector3(radius, radius, radius) * (1f + pulse * pulseSwell);
            if (activeRenderer == null)
                return;

            tintBlock ??= new MaterialPropertyBlock();
            activeRenderer.GetPropertyBlock(tintBlock);
            tintBlock.SetFloat(RimIntensityId, pulse * pulseRim);
            activeRenderer.SetPropertyBlock(tintBlock);
        }

        public void SetVisible(bool value)
        {
            visible = value;
            if (activeRenderer != null)
                activeRenderer.enabled = value;
        }

        private void SetActiveRendererBySeed(int seed)
        {
            if (renderers == null || renderers.Count == 0)
                return;

            int startIndex = (int)((uint)seed % (uint)renderers.Count);
            for (int i = 0; i < renderers.Count; i++)
            {
                int index = (startIndex + i) % renderers.Count;
                if (renderers[index] == null)
                    continue;

                SetActiveRenderer(index);
                return;
            }
        }

        private void SetActiveRenderer(int index)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                    renderers[i].gameObject.SetActive(i == index);
            }
            activeRenderer = renderers[index];
            baseColor = BaseColorOf(activeRenderer);
            activeRenderer.enabled = visible;
            if (hasRouteColor || hasTint)
                ApplyBaseColor();
        }

        private void EnsureReadyForRouteColor()
        {
            if (activeRenderer != null || renderers == null || renderers.Count == 0)
                return;

            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] == null)
                    continue;

                SetActiveRenderer(i);
                return;
            }
        }

        public void SetRouteColor(Color color)
        {
            EnsureReadyForRouteColor();
            routeColor = color;
            hasRouteColor = true;
            ApplyBaseColor();
        }

        public void SetTint(Color color)
        {
            tintColor = color;
            hasTint = true;
            ApplyBaseColor();
        }

        public void ClearTint()
        {
            hasTint = false;
            ApplyBaseColor();
        }

        private void ApplyBaseColor()
        {
            if (activeRenderer == null)
                return;

            tintBlock ??= new MaterialPropertyBlock();
            activeRenderer.GetPropertyBlock(tintBlock);
            Color color = baseColor;
            if (hasRouteColor)
                color = routeColor;
            if (hasTint)
                color = tintColor;

            tintBlock.SetColor(BaseColorId, color);
            tintBlock.SetColor(ColorId, color);
            activeRenderer.SetPropertyBlock(tintBlock);
        }

        private static Color BaseColorOf(Renderer renderer)
        {
            Material material = renderer.sharedMaterial;
            if (material == null)
                return Color.white;

            return material.HasProperty(BaseColorId)
                ? material.GetColor(BaseColorId)
                : material.color;
        }
    }
}
