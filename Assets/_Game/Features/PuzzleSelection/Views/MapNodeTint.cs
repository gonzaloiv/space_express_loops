using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    internal class MapNodeTint
    {
        private readonly SelectionCubePalette palette;
        private readonly float lockedBrightness;
        private readonly float pulseSecs;
        private readonly float pulseDepth;
        private float phase;

        public MapNodeTint(
            SelectionCubePalette palette, float lockedBrightness,
            float pulseSecs, float pulseDepth)
        {
            this.palette = palette;
            this.lockedBrightness = lockedBrightness;
            this.pulseSecs = Mathf.Max(pulseSecs, 0.01f);
            this.pulseDepth = pulseDepth;
        }

        public Color For(JobState state)
        {
            switch (state)
            {
                case JobState.Solved:
                    return palette.Solved.value;
                case JobState.Current:
                    return palette.Current.value;
                default:
                    return Dim(palette.Locked.value, lockedBrightness);
            }
        }

        public Color CurrentAt(float deltaSecs)
        {
            phase += deltaSecs;
            float wave = 0.5f + 0.5f * Mathf.Sin(phase * Mathf.PI * 2f / pulseSecs);
            return Dim(palette.Current.value, Mathf.Lerp(1f - pulseDepth, 1f, wave));
        }

        public void Reset() => phase = 0f;

        private static Color Dim(Color color, float amount) =>
            new(color.r * amount, color.g * amount, color.b * amount, color.a);
    }
}