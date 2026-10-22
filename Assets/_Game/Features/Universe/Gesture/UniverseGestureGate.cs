using DigitalLove.Input;
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

        public bool UpdateSuppression(TwoHandGestureState state, UniverseGhostView ghostView)
        {
            if (!suppressUntilRelease)
                return false;

            ghostView.Hide();
            suppressUntilRelease = state.IsAnyPinching;
            return true;
        }

        public void OnTransitionCompleted(TwoHandGestureState state)
        {
            BeginCooldown();
            if (suppressUntilRelease && !state.IsAnyPinching)
                suppressUntilRelease = false;
        }
    }
}
