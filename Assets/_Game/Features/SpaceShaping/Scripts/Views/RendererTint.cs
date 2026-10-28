using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    internal struct RendererTint
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock block;

        public void Set(Renderer target, Color color)
        {
            if (target == null)
                return;

            block ??= new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            block.SetColor(BaseColorId, color);
            block.SetColor(ColorId, color);
            target.SetPropertyBlock(block);
        }
    }
}
