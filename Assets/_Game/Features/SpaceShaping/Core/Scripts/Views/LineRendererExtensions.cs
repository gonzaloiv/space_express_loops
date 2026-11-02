using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public static class LineRendererExtensions
    {
        private static readonly MaterialPropertyBlock Block = new();

        public static void SetBaseColor(this LineRenderer line, Color color)
        {
            Block.Clear();
            Block.SetColor("_BaseColor", color);
            Block.SetColor("_Color", color);
            line.SetPropertyBlock(Block);
        }
    }
}
