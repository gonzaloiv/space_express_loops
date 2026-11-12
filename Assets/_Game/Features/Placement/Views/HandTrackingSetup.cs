using System;
using System.Collections;
using DigitalLove.Casual.Flow.Setup;
using Oculus.Interaction.Input;
using UnityEngine;

namespace DigitalLove.Game.Placement
{
    // The boot's hands gate: waits for the headset to be worn, then for hand tracking and both hands
    // to stay tracked through the steady window. The callback lives outside the wait so a re-wear can
    // restart the wait without dropping setup's continuation.
    public class HandTrackingSetup : SetupCheck
    {
        [SerializeField] private Hand leftHand;
        [SerializeField] private Hand rightHand;
        [SerializeField] private HandTrackingRequiredPanel panel;

        private readonly HandTrackingGate gate = new();
        private Action pendingDone;
        private Coroutine waiting;
        private bool panelShown;

        public override void Setup(Action onComplete)
        {
            pendingDone = onComplete;
            gate.Reset();

            if (Application.isEditor)
            {
                Complete();
                return;
            }

            if (waiting != null)
                StopCoroutine(waiting);

            waiting = StartCoroutine(WaitForHands());
        }

        private IEnumerator WaitForHands()
        {
            while (true)
            {
                if (!HeadsetPresence.IsWorn())
                {
                    ResetWhileOffHead();
                    yield return null;
                    continue;
                }

                if (TryComplete())
                    yield break;

                yield return null;
            }
        }

        private void ResetWhileOffHead()
        {
            gate.Reset();
            SetPanel(false);
        }

        private bool TryComplete()
        {
            HandTrackingStatus status = gate.Tick(
                OVRPlugin.GetHandTrackingEnabled(),
                IsTracked(leftHand),
                IsTracked(rightHand),
                Time.unscaledDeltaTime);
            SetPanel(status.PanelDue);

            if (status.State != HandTrackingState.Ready)
                return false;

            Complete();
            return true;
        }

        private void Complete()
        {
            SetPanel(false);
            Action done = pendingDone;
            pendingDone = null;
            done?.Invoke();
        }

        private void SetPanel(bool shown)
        {
            if (shown == panelShown)
                return;

            panelShown = shown;
            if (shown)
                panel.Show();
            else
                panel.Hide();
        }

        private static bool IsTracked(Hand hand) => hand.IsConnected && hand.IsTrackedDataValid;
    }
}
