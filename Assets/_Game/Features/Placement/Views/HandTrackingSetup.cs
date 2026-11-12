using System;
using System.Collections;
using DigitalLove.Casual.Flow.Setup;
using DigitalLove.Game.UI.Console;
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
        [Tooltip("The boot console's hand-tracking lines; this gate only says when they are due.")]
        [SerializeField] private ConsoleHint hint;

        private readonly HandTrackingGate gate = new();
        private Action pendingDone;
        private Coroutine waiting;

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
            hint.Due = false;
        }

        private bool TryComplete()
        {
            HandTrackingStatus status = gate.Tick(
                OVRPlugin.GetHandTrackingEnabled(),
                IsTracked(leftHand),
                IsTracked(rightHand),
                Time.unscaledDeltaTime);
            hint.Due = status.HintDue;

            if (status.State != HandTrackingState.Ready)
                return false;

            Complete();
            return true;
        }

        private void Complete()
        {
            hint.Due = false;
            Action done = pendingDone;
            pendingDone = null;
            done?.Invoke();
        }

        private static bool IsTracked(Hand hand) => hand.IsConnected && hand.IsTrackedDataValid;
    }
}
