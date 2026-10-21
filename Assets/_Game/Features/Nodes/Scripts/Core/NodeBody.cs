using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class NodeBody : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Transform body;
        [SerializeField] private List<Renderer> renderers;
        [SerializeField] private Color defaultColor = Color.white;

        private Renderer activeRenderer;

        public float Radius => body.lossyScale.x;
        public Vector3 Position => body.position;

        public void Init(float radius) =>
            Init(radius, Random.Range(0, int.MaxValue));

        public void Init(float radius, int variantSeed)
        {
            body.localScale = new Vector3(radius, radius, radius);
            SetActiveRendererBySeed(variantSeed);
        }

        public void SetVisible(bool visible)
        {
            if (activeRenderer != null)
                activeRenderer.enabled = visible;
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
        }

        public void EnsureReadyForRouteColor()
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

        public void ResetRouteColor()
        {
            SetRouteColor(defaultColor);
        }

    }
}
