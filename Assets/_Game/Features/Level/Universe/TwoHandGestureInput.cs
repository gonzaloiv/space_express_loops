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
        [SerializeField, Min(0f)] private float dwellSecs = 0.12f;
        [SerializeField, Min(0f)] private float releaseGraceSecs = 0.15f;

        private Hand leftHand;
        private Hand rightHand;
        private bool isActive;
        private float bothPinchingSince = -1f;
        private float lastBothPinchTime;
        private float previousHandDistance;
        private bool hasPreviousHandDistance;

        public bool IsActive => isActive;
        public bool IsLeftPinching { get; private set; }
        public bool IsRightPinching { get; private set; }
        public bool IsSingleHandPinching => IsLeftPinching != IsRightPinching;
        public bool IsAnyPinching => IsLeftPinching || IsRightPinching;
        public Vector3 DragPoint => IsLeftPinching
            ? LeftPinchPoint
            : IsRightPinching
                ? RightPinchPoint
                : Midpoint;
        public Vector3 LeftPinchPoint { get; private set; }
        public Vector3 RightPinchPoint { get; private set; }
        public Vector3 Midpoint => (LeftPinchPoint + RightPinchPoint) * 0.5f;
        public float HandDistance => Vector3.Distance(LeftPinchPoint, RightPinchPoint);
        public float HandDistanceSpeed { get; private set; }

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
            bool isBusy = IsHandBusy();

            if (hasLeft)
                LeftPinchPoint = leftPoint;
            if (hasRight)
                RightPinchPoint = rightPoint;

            IsLeftPinching = hasLeft && !isBusy && IsPinching(leftHand);
            IsRightPinching = hasRight && !isBusy && IsPinching(rightHand);

            bool isTracked = hasLeft && hasRight;
            bool isPinching = IsLeftPinching && IsRightPinching;

            UpdateDistanceSpeed(isPinching);
            UpdatePinchState(isPinching);

            if (isActive)
                UpdateActive(isTracked, isPinching);
            else
                TryActivate(isPinching);
        }

        private void UpdateDistanceSpeed(bool isPinching)
        {
            float distance = HandDistance;
            if (!isPinching || Time.deltaTime <= 0f)
            {
                HandDistanceSpeed = 0f;
                previousHandDistance = distance;
                hasPreviousHandDistance = true;
                return;
            }

            if (hasPreviousHandDistance)
                HandDistanceSpeed = Mathf.Abs(distance - previousHandDistance) / Time.deltaTime;

            previousHandDistance = distance;
            hasPreviousHandDistance = true;
        }

        private void UpdatePinchState(bool isPinching)
        {
            if (!isPinching)
            {
                bothPinchingSince = -1f;
                return;
            }

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
