using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    // The frame's corner brackets: eases how far they reach along each edge toward a target, so the
    // cube locks on when a hand comes near and lights its whole frame while it is held.
    public sealed class CubeFrameBrackets
    {
        private static readonly int BracketId = Shader.PropertyToID("_Bracket");

        private readonly Renderer frame;
        private MaterialPropertyBlock block;
        private float current = -1f;

        public CubeFrameBrackets(Renderer frame) => this.frame = frame;

        public void Ease(float target, float secs)
        {
            float step = secs > 0f ? Time.unscaledDeltaTime / secs : 1f;
            float next = current < 0f ? target : Mathf.MoveTowards(current, target, step);
            if (Mathf.Approximately(next, current))
                return;

            current = next;
            block ??= new MaterialPropertyBlock();
            frame.GetPropertyBlock(block);
            block.SetFloat(BracketId, current);
            frame.SetPropertyBlock(block);
        }
    }
}
