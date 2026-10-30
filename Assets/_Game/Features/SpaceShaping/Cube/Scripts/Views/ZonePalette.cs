using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    // The zone's colour scheme: frame and fill tints for prediction, refusal, hover, and scale.
    public sealed class ZonePalette
    {
        public Color Neutral;
        public Color NeutralHover;
        public Color Slow;
        public Color Fast;
        public Color Success;
        public Color Miss;
        public Color Danger;
        public float FillIdleAlpha = 0.1f;
        public float FillActiveAlpha = 0.45f;
        public float RefuseSecs = 0.18f;
    }
}
