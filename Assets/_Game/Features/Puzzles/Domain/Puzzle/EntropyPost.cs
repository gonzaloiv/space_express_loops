using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public enum StarHold
    {
        Held,
        AtRisk,
        Lost
    }

    // The entropy post's reading, as heights from its foot (0) to its top (1). What remains is
    // [0, PreviewFrom), the pinch's cost [PreviewFrom, RemainingTop), the last edit up to LastEditTop,
    // and what is spent above. A star at a use limit holds while the top stays at or above its height.
    public readonly struct EntropyPost
    {
        public readonly float PreviewFrom;
        public readonly float RemainingTop;
        public readonly float LastEditTop;
        public readonly bool Overflow;

        // Shares of the budget: committed, the pinch in flight, the last edit.
        public EntropyPost(float committed, float pending, float lastEdit, bool overflow)
        {
            RemainingTop = 1f - Mathf.Clamp01(committed);
            PreviewFrom = Mathf.Max(RemainingTop - Mathf.Max(pending, 0f), 0f);
            LastEditTop = Mathf.Min(RemainingTop + Mathf.Max(lastEdit, 0f), 1f);
            Overflow = overflow;
        }

        // Where a use limit sits on the post: spending past it drops the top below it.
        public static float HeightOf(float useLimit) => 1f - Mathf.Clamp01(useLimit);

        public StarHold Star(float useLimit)
        {
            if (useLimit >= 1f)
                return Overflow ? StarHold.AtRisk : StarHold.Held;

            float height = HeightOf(useLimit);
            if (RemainingTop < height)
                return StarHold.Lost;

            return PreviewFrom < height ? StarHold.AtRisk : StarHold.Held;
        }
    }
}
