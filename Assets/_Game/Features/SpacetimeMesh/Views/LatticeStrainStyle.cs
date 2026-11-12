using System;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    // How the lattice shows strain: where the warp squeezes the lines together they thicken and glow
    // toward the strain colour, as if space were being crushed; where it stretches them apart they thin
    // and fade. The shape stays the physics' warp; only the line's look changes.
    [Serializable]
    public class LatticeStrainStyle
    {
        private static readonly int StrainColorId = Shader.PropertyToID("_StrainColor");

        [Tooltip("Spacing (fraction of rest) at which a line is fully squeezed.")]
        [Range(0.1f, 0.95f)] public float squeezeAt = 0.6f;

        [Tooltip("Spacing (multiple of rest) at which a line is fully stretched.")]
        [Min(1.05f)] public float stretchAt = 1.8f;

        [Tooltip("Extra tube radius when fully squeezed: 1.5 makes it 2.5× as thick.")]
        [Min(0f)] public float thicken = 1.5f;

        [Tooltip("Radius lost when fully stretched: 0.5 halves it.")]
        [Range(0f, 1f)] public float thin = 0.5f;

        [Tooltip("Opacity lost when fully stretched.")]
        [Range(0f, 1f)] public float stretchFade = 0.5f;

        [Tooltip("Opacity of a fully squeezed line (the rest of the lattice uses the grid opacity).")]
        [Range(0f, 1f)] public float glowOpacity = 0.9f;

        [NonSerialized] private MaterialPropertyBlock block;

        public void Measure(Vector3[] rest, Vector3[] warped, float[] strain, int samplesPerLine) =>
            LatticeStrain.Measure(rest, warped, strain, samplesPerLine, squeezeAt, stretchAt);

        public float Radius(float radius, float strain) =>
            strain >= 0f ? radius * (1f + thicken * strain) : radius * (1f + thin * strain);

        // Red carries the squeeze to the shader (0 = base colour, 1 = strain colour); alpha the fade.
        public Color VertexColor(float fade, float strain) =>
            new(Mathf.Max(strain, 0f), 0f, 0f, fade * (1f + stretchFade * Mathf.Min(strain, 0f)));

        public void ApplyGlow(Renderer target, Color glow)
        {
            block ??= new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            glow.a = glowOpacity;
            block.SetColor(StrainColorId, glow);
            target.SetPropertyBlock(block);
        }
    }
}
