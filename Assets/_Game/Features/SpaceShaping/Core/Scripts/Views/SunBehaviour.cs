using DigitalLove.Game.Nodes;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Keeps the authored corona proportional to the sun and moves the growth it cannot show in size
    // into brightness, so a scaled sun does not swallow the inner ring.
    public class SunBehaviour : MonoBehaviour
    {
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

        [SerializeField] private FieldPlanetBehaviour planet;
        [SerializeField] private Transform corona;
        [SerializeField] private Renderer coronaRenderer;
        [SerializeField] private float coronaRadiusFactor = 1.6f;
        [SerializeField] private float maxCoronaDiameter = 0.26f;
        [SerializeField] private float baseIntensity = 2f;
        [SerializeField] private float intensityBoost = 1.5f;

        private MaterialPropertyBlock block;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            coronaRenderer.enabled = false;
        }

        private void LateUpdate()
        {
            bool visible = planet.IsVisible;
            if (coronaRenderer.enabled != visible)
                coronaRenderer.enabled = visible;

            // NodeBody scales the sun's root, so the corona is a child of a transform that is already
            // the body's diameter: divide the wanted world size by it to get a local scale.
            float radius = planet.Radius;
            float diameter = Mathf.Min(radius * coronaRadiusFactor, maxCoronaDiameter);
            corona.localScale = Vector3.one * (radius > 0f ? diameter / radius : 0f);

            float hidden = diameter > 0f ? 1f - diameter / (radius * coronaRadiusFactor) : 0f;
            coronaRenderer.GetPropertyBlock(block);
            block.SetFloat(IntensityId, baseIntensity * (1f + hidden * intensityBoost));
            coronaRenderer.SetPropertyBlock(block);
        }
    }
}
