using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class NodeBody : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Transform body;
        [SerializeField] private List<Renderer> renderers;

        private Renderer activeRenderer;
        private MaterialPropertyBlock tintBlock;
        private bool visible = true;

        public void Init(float radius, int variantSeed)
        {
            SetRadius(radius);
            SetActiveRendererBySeed(variantSeed);
        }

        public void SetRadius(float radius) =>
            body.localScale = new Vector3(radius, radius, radius);

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
            activeRenderer.enabled = visible;
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
            if (activeRenderer == null)
                return;
            Material material = activeRenderer.material;
            material.SetColor(BaseColorId, color);
            material.color = color;
        }

        public void SetTint(Color color)
        {
            if (activeRenderer == null)
                return;

            tintBlock ??= new MaterialPropertyBlock();
            activeRenderer.GetPropertyBlock(tintBlock);
            tintBlock.SetColor(BaseColorId, color);
            tintBlock.SetColor(ColorId, color);
            activeRenderer.SetPropertyBlock(tintBlock);
        }

        public void ClearTint()
        {
            if (activeRenderer != null)
                activeRenderer.SetPropertyBlock(null);
        }

    }
}
