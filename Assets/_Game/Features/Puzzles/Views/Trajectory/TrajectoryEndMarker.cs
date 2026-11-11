using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The line's end: one authored marker, positioned at the path's end and tinted by outcome.
    public sealed class TrajectoryEndMarker
    {
        private readonly Transform root;
        private readonly Renderer renderer;
        private readonly ColorValue success;
        private readonly ColorValue danger;
        private readonly ColorValue neutral;
        private RendererTint tint;

        public TrajectoryEndMarker(
            Transform root, Renderer renderer, ColorValue success, ColorValue danger, ColorValue neutral)
        {
            this.root = root;
            this.renderer = renderer;
            this.success = success;
            this.danger = danger;
            this.neutral = neutral;
        }

        public bool IsActive => root.gameObject.activeSelf;

        public void Show(Vector3 position, Vector3 heading, Color color)
        {
            tint.Set(renderer, color);
            root.localPosition = position;
            if (heading.sqrMagnitude > 1e-10f)
                root.localRotation = Quaternion.LookRotation(heading, Vector3.up);

            root.gameObject.SetActive(true);
        }

        public Color ColorFor(TrajectoryOutcome outcome)
        {
            if (outcome == TrajectoryOutcome.Arrive)
                return success.value;

            return outcome == TrajectoryOutcome.Crash ? danger.value : neutral.value;
        }

        public void Hide() => root.gameObject.SetActive(false);
    }
}
