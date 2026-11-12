using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    // The zone's colour scheme: frame and fill tints for prediction, refusal, hover, and scale, and
    // how far the frame's corner brackets reach in each state.
    public sealed class CubePalette
    {
        public Color Neutral;
        public Color Accent;
        public Color Success;
        public Color Danger;
        public float FillIdleAlpha = 0.1f;
        public float FillActiveAlpha = 0.45f;
        public float DormantFrameAlpha = 0.6f;
        public float RefuseSecs = 0.18f;
        public float BracketIdle = 0.16f;
        public float BracketHover = 0.3f;
        public float BracketActive = 0.6f;
        public float BracketSecs = 0.15f;
    }
}
