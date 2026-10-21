using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;

namespace DigitalLove.Game.Levels
{
    [DefaultExecutionOrder(-100)]
    public class TwoHandGestureInput : MonoBehaviour
    {
        [Header("Hands")]
        [SerializeField] private MonoBehaviour leftHandSource;
        [SerializeField] private MonoBehaviour rightHandSource;

        [Header("Busy hands")]
        [SerializeField] private MonoBehaviour[] busyInteractors;

        [Header("Pinch")]
        [SerializeField, Range(0f, 1f)] private float pinchStrengthThreshold = 0.75f;
        [SerializeField, Min(0f)] private float dwellSecs;
        [SerializeField, Min(0f)] private float releaseGraceSecs = 0.15f;

        private Hand leftHand;
        private Hand rightHand;
        private bool isActive;
        private float bothPinchingSince = -1f;
        private float lastBothPinchTime;

        public bool IsActive => isActive;
        public Vector3 LeftPinchPoint { get; private set; }
        public Vector3 RightPinchPoint { get; private set; }
        public Vector3 Midpoint => (LeftPinchPoint + RightPinchPoint) * 0.5f;
        public float HandDistance => Vector3.Distance(LeftPinchPoint, RightPinchPoint);

        private void Awake()
        {
            leftHand = leftHandSource as Hand;
            rightHand = rightHandSource as Hand;

            if (leftHand == null || rightHand == null)
            {
                Debug.LogError("[TwoHandGestureInput] Hand sources must reference Oculus.Interaction.Input.Hand components.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            bool hasLeft = TryGetPinchPoint(leftHand, out Vector3 leftPoint);
            bool hasRight = TryGetPinchPoint(rightHand, out Vector3 rightPoint);
            bool isTracked = hasLeft && hasRight;
            bool isPinching = isTracked && IsPinching(leftHand) && IsPinching(rightHand) && !IsHandBusy();

            UpdatePinchState(isPinching, leftPoint, rightPoint);

            if (isActive)
                UpdateActive(isTracked, isPinching);
            else
                TryActivate(isPinching);
        }

        private void UpdatePinchState(bool isPinching, Vector3 leftPoint, Vector3 rightPoint)
        {
            if (!isPinching)
            {
                bothPinchingSince = -1f;
                return;
            }

            LeftPinchPoint = leftPoint;
            RightPinchPoint = rightPoint;
            lastBothPinchTime = Time.unscaledTime;
            if (bothPinchingSince < 0f)
                bothPinchingSince = Time.unscaledTime;
        }

        private void UpdateActive(bool isTracked, bool isPinching)
        {
            bool lostTracking = !isTracked;
            bool released = !isPinching && Time.unscaledTime - lastBothPinchTime > releaseGraceSecs;
            if (lostTracking || released)
                isActive = false;
        }

        private void TryActivate(bool isPinching)
        {
            if (!isPinching || bothPinchingSince < 0f)
                return;

            if (Time.unscaledTime - bothPinchingSince < dwellSecs)
                return;

            isActive = true;
        }

        private bool IsPinching(Hand hand) =>
            hand.GetFingerPinchStrength(HandFinger.Index) >= pinchStrengthThreshold;

        private static bool TryGetPinchPoint(Hand hand, out Vector3 point)
        {
            point = default;
            if (!hand.IsConnected || !hand.IsTrackedDataValid)
                return false;

            if (!hand.GetJointPose(HandJointId.HandThumbTip, out Pose thumbPose)
                || !hand.GetJointPose(HandJointId.HandIndexTip, out Pose indexPose))
            {
                return false;
            }

            point = (thumbPose.position + indexPose.position) * 0.5f;
            return true;
        }

        private bool IsHandBusy()
        {
            for (int i = 0; i < busyInteractors.Length; i++)
            {
                if (IsSelecting(busyInteractors[i]))
                    return true;
            }

            return false;
        }

        private static bool IsSelecting(MonoBehaviour interactor) =>
            interactor is IInteractorView view && view.State == InteractorState.Select;
    }
}
