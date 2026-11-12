namespace DigitalLove.Game.Placement
{
    // A boot gate: ready once both hands have tracked together for a steady window. Hand tracking
    // off, or either hand missing, restarts the window and keeps the gate waiting.
    public sealed class HandTrackingGate
    {
        public const float SteadySecs = 0.3f;

        private float steadySecs;

        public void Reset()
        {
            steadySecs = 0f;
        }

        public bool Tick(bool enabled, bool leftTracked, bool rightTracked, float deltaSecs)
        {
            if (!enabled || !leftTracked || !rightTracked)
            {
                Reset();
                return false;
            }

            if (deltaSecs > 0f)
                steadySecs += deltaSecs;

            return steadySecs >= SteadySecs;
        }
    }
}
