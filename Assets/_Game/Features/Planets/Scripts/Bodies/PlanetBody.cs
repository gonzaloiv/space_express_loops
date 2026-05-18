using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class PlanetBody : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Transform body;
        [SerializeField] private List<Renderer> renderers;
        [SerializeField] private Color defaultColor = Color.white;

        private Renderer activeRenderer;

        public float Radius => body.lossyScale.x;
        public Vector3 Position => body.position;

        public void Init(float radius)
        {
            body.localScale = new Vector3(radius, radius, radius);
            SetRandomActiveRenderer();
        }

        private void SetRandomActiveRenderer()
        {
            if (renderers == null || renderers.Count == 0)
                return;

            int index = renderers.Count > 1 ? Random.Range(0, renderers.Count) : 0;
            SetActiveRenderer(index);
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
            EnsureActiveRenderer();
            if (activeRenderer != null)
                return;

            if (renderers != null && renderers.Count > 0)
                SetActiveRenderer(0);
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

        private void EnsureActiveRenderer()
        {
            if (activeRenderer != null)
                return;

            if (renderers == null || renderers.Count == 0)
                return;

            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null && renderers[i].gameObject.activeInHierarchy)
                {
                    activeRenderer = renderers[i];
                    return;
                }
            }

            activeRenderer = renderers[0];
        }
    }
}
