using System;
using System.Collections;
using DigitalLove.Casual.Flow.Setup;
using DigitalLove.XR;
using Oculus.Interaction.Input;
using UnityEngine;

namespace DigitalLove.Game.Placement
{
    // Places the field where the seated player actually is. Setup waits until the headset is worn and
    // the head has settled (a headset lying on the desk at launch places nothing), then places from
    // the head and the resting hands, then lays the console arc around the real head. Re-places when the headset goes back on and on a system recenter;
    // a job start keeps the placement. The editor keeps the scene's authored layout.
    public class SeatPlacement : XRSpaceSetup
    {
        [SerializeField] private OVROriginBehaviour origin;
        [SerializeField] private Transform head;
        [SerializeField] private Hand leftHand;
        [SerializeField] private Hand rightHand;
        [SerializeField] private OVRSessionHelper session;
        [SerializeField] private SeatSettings settings = SeatSettings.Default;

        [Header("Settling")]
        [Tooltip("The head must turn slower than this (degrees per second)...")]
        [SerializeField, Min(1f)] private float settledDegPerSec = 30f;
        [Tooltip("...for this long before the field is placed.")]
        [SerializeField, Min(0f)] private float settledSecs = 0.4f;
        [Tooltip("Place anyway after this long worn, even if the head keeps moving.")]
        [SerializeField, Min(0f)] private float maxSettleSecs = 3f;

        private Coroutine placing;

        // Setup's callback survives a restart: putting the headset on mid-wait restarts the wait,
        // and setup must still hear about the placement that follows.
        private Action pendingPlaced;

        public override void Setup(Action onComplete)
        {
            pendingPlaced = onComplete;
            PlaceWhenWorn();
        }

        private void OnEnable()
        {
            OVRManager.HMDMounted += OnMounted;
            session.hasRecenteredPose += Place;
        }

        private void OnDisable()
        {
            OVRManager.HMDMounted -= OnMounted;
            session.hasRecenteredPose -= Place;
        }

        private void OnMounted() => PlaceWhenWorn();

        private void PlaceWhenWorn()
        {
            if (placing != null)
                StopCoroutine(placing);
            placing = StartCoroutine(WaitAndPlace());
        }

        private IEnumerator WaitAndPlace()
        {
            while (!IsWorn())
                yield return null;

            yield return Settle();
            Place();
            placing = null;
            Action placed = pendingPlaced;
            pendingPlaced = null;
            placed?.Invoke();
        }

        private IEnumerator Settle()
        {
            float worn = 0f;
            float still = 0f;
            Quaternion last = head.rotation;
            while (still < settledSecs && worn < maxSettleSecs)
            {
                yield return null;
                float dt = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
                float speed = Quaternion.Angle(last, head.rotation) / dt;
                last = head.rotation;
                worn += dt;
                still = speed < settledDegPerSec ? still + dt : 0f;
            }
        }

        private void Place()
        {
            if (Application.isEditor)
                return;

            SeatPose pose = SeatPose.From(
                head.position, head.rotation, PalmOf(leftHand), PalmOf(rightHand), settings);
            origin.Setup(pose.Position, pose.Forward);
        }

        private static bool IsWorn() =>
            Application.isEditor || OVRManager.instance == null || OVRManager.instance.isUserPresent;

        private static Vector3? PalmOf(Hand hand)
        {
            if (!hand.IsConnected || !hand.IsTrackedDataValid)
                return null;

            return hand.GetJointPose(HandJointId.HandPalm, out Pose palm) ? palm.position : null;
        }
    }
}
