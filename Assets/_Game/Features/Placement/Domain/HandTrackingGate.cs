namespace DigitalLove.Game.Placement
{
    public enum HandTrackingState
    {
        Missing,
        Settling,
        Ready
    }

    public readonly struct HandTrackingStatus
    {
        public HandTrackingStatus(HandTrackingState state, bool hintDue)
        {
            State = state;
            HintDue = hintDue;
        }

        public HandTrackingState State { get; }
        public bool HintDue { get; }
    }

    // A boot gate: ready once both hands have tracked together for a steady window. Hand tracking
    // off, or either hand missing, restarts the window and keeps the gate waiting. The console hint is due
    // only after a longer missing period, so the first frames and a short flicker never type it.
    public sealed class HandTrackingGate
    {
        public const float SteadySecs = 0.3f;
        public const float HintDelaySecs = 0.5f;

        private float steadySecs;
        private float missingSecs;

        public void Reset()
        {
            steadySecs = 0f;
            missingSecs = 0f;
        }

        public HandTrackingStatus Tick(bool enabled, bool leftTracked, bool rightTracked, float deltaSecs)
        {
            if (!enabled || !leftTracked || !rightTracked)
                return Missing(deltaSecs);

            missingSecs = 0f;
            if (deltaSecs > 0f)
                steadySecs += deltaSecs;

            HandTrackingState state = steadySecs >= SteadySecs
                ? HandTrackingState.Ready
                : HandTrackingState.Settling;
            return new HandTrackingStatus(state, false);
        }

        private HandTrackingStatus Missing(float deltaSecs)
        {
            steadySecs = 0f;
            if (deltaSecs > 0f)
                missingSecs += deltaSecs;

            return new HandTrackingStatus(
                HandTrackingState.Missing,
                missingSecs >= HintDelaySecs);
        }
    }
}
