using UnityEngine;

namespace DigitalLove.Game.Universe
{
    internal class UniverseGestureGate
    {
        private readonly float cooldownSecs;
        private bool suppressUntilRelease;
        private float cooldownUntil;

        public UniverseGestureGate(float cooldownSecs)
        {
            this.cooldownSecs = cooldownSecs;
        }

        public bool IsCoolingDown => Time.unscaledTime < cooldownUntil;

        public void BeginCooldown() => cooldownUntil = Time.unscaledTime + cooldownSecs;

        public void BeginCooldown(float minSecs) =>
            cooldownUntil = Time.unscaledTime + Mathf.Max(cooldownSecs, minSecs);

        public void SuppressUntilRelease() => suppressUntilRelease = true;

        public bool UpdateSuppression(TwoHandGestureInput input, UniverseGhostView ghostView)
        {
            if (!suppressUntilRelease)
                return false;

            ghostView.Hide();
            suppressUntilRelease = input.IsAnyPinching;
            return true;
        }

        public void OnTransitionCompleted(TwoHandGestureInput input)
        {
            BeginCooldown();
            if (suppressUntilRelease && !input.IsAnyPinching)
                suppressUntilRelease = false;
        }
    }
}
