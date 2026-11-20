using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    // The strain style's numbers as a plain value, so the Burst ring job can shape and colour a line.
    public readonly struct LatticeStrainLook
    {
        public readonly float SqueezeAt;
        public readonly float StretchAt;
        private readonly float thicken;
        private readonly float thin;
        private readonly float stretchFade;

        public LatticeStrainLook(
            float squeezeAt, float stretchAt, float thicken, float thin, float stretchFade)
        {
            SqueezeAt = squeezeAt;
            StretchAt = stretchAt;
            this.thicken = thicken;
            this.thin = thin;
            this.stretchFade = stretchFade;
        }

        public float Radius(float radius, float strain) =>
            strain >= 0f ? radius * (1f + thicken * strain) : radius * (1f + thin * strain);

        // Red carries the squeeze to the shader (0 = base colour, 1 = strain colour); alpha the fade.
        public Color VertexColor(float fade, float strain) =>
            new(Mathf.Max(strain, 0f), 0f, 0f, fade * (1f + stretchFade * Mathf.Min(strain, 0f)));
    }
}
