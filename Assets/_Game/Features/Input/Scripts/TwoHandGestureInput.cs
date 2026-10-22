using System;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;

namespace DigitalLove.Input
{
    [DefaultExecutionOrder(-100)]
    public class TwoHandGestureInput : MonoBehaviour
    {
        [Header("Hands")]
        [SerializeField] private Hand leftHandSource;
        [SerializeField] private Hand rightHandSource;

        [Header("Busy hands")]
        [SerializeField] private MonoBehaviour[] busyInteractors;

        [Header("Pinch")]
        [SerializeField, Range(0f, 1f)] private float pinchStrengthThreshold = 0.75f;
        [SerializeField, Min(0f)] private float dwellSecs = 0.12f;
        [SerializeField, Min(0f)] private float releaseGraceSecs = 0.15f;

        private bool isActive;
        private float bothPinchingSince = -1f;
        private float lastBothPinchTime;
        private float previousHandDistance;
        private float handDistanceSpeed;
        private bool hasPreviousHandDistance;
        private bool wasLeftPinching;
        private bool wasRightPinching;
        private bool hasLeftHand;
        private bool hasRightHand;
        private bool isLeftPinching;
        private bool isRightPinching;
        private bool leftStarted;
        private bool rightStarted;
        private bool leftEnded;
        private bool rightEnded;
        private float handDistance;
        private Vector3 leftPinchPoint;
        private Vector3 rightPinchPoint;
        private Vector3 leftPinchStartPoint;
        private Vector3 rightPinchStartPoint;

        public TwoHandGestureState State { get; private set; }
        public event Action<TwoHandGestureState> StateChanged;
        public event Action<TwoHandGestureState> GestureStarted;
        public event Action<TwoHandGestureState> GestureEnded;
        public event Action<HandSide, Vector3> PinchStarted;
        public event Action<HandSide> PinchEnded;

        private void Update()
        {
            hasLeftHand = TryGetPinchPoint(leftHandSource, out Vector3 leftPoint);
            hasRightHand = TryGetPinchPoint(rightHandSource, out Vector3 rightPoint);
            bool isBusy = IsHandBusy();

            if (hasLeftHand)
                leftPinchPoint = leftPoint;
            if (hasRightHand)
                rightPinchPoint = rightPoint;

            isLeftPinching = hasLeftHand && !isBusy && IsPinching(leftHandSource);
            isRightPinching = hasRightHand && !isBusy && IsPinching(rightHandSource);
            handDistance = Vector3.Distance(leftPinchPoint, rightPinchPoint);

            UpdateDistanceSpeed(isLeftPinching && isRightPinching);
            UpdateActive(isLeftPinching && isRightPinching, hasLeftHand && hasRightHand);
            Publish();
        }

        private void UpdateDistanceSpeed(bool isPinching)
        {
            if (!isPinching || Time.deltaTime <= 0f)
            {
                handDistanceSpeed = 0f;
                previousHandDistance = handDistance;
                hasPreviousHandDistance = true;
                return;
            }

            if (hasPreviousHandDistance)
                handDistanceSpeed = Mathf.Abs(handDistance - previousHandDistance) / Time.deltaTime;

            previousHandDistance = handDistance;
            hasPreviousHandDistance = true;
        }

        private void UpdateActive(bool bothPinching, bool isTracked)
        {
            if (!bothPinching)
            {
                bothPinchingSince = -1f;
                if (isActive && (!isTracked || Time.unscaledTime - lastBothPinchTime > releaseGraceSecs))
                    isActive = false;
                return;
            }

            lastBothPinchTime = Time.unscaledTime;
            if (bothPinchingSince < 0f)
                bothPinchingSince = Time.unscaledTime;

            if (!isActive && Time.unscaledTime - bothPinchingSince >= dwellSecs)
                isActive = true;
        }

        private void Publish()
        {
            bool wasActive = isActive;
            UpdatePinchTransitions();
            TwoHandGestureState next = new(
                new HandPinchState(hasLeftHand, isLeftPinching, leftPinchPoint, leftPinchStartPoint),
                new HandPinchState(hasRightHand, isRightPinching, rightPinchPoint, rightPinchStartPoint),
                isActive,
                handDistance,
                handDistanceSpeed);
            State = next;
            RaiseEvents(next, wasActive);
        }

        private void UpdatePinchTransitions()
        {
            leftStarted = isLeftPinching && !wasLeftPinching;
            rightStarted = isRightPinching && !wasRightPinching;
            leftEnded = !isLeftPinching && wasLeftPinching;
            rightEnded = !isRightPinching && wasRightPinching;

            if (leftStarted)
                leftPinchStartPoint = leftPinchPoint;
            if (rightStarted)
                rightPinchStartPoint = rightPinchPoint;

            wasLeftPinching = isLeftPinching;
            wasRightPinching = isRightPinching;
        }

        private void RaiseEvents(TwoHandGestureState next, bool wasActive)
        {
            if (leftStarted)
                PinchStarted?.Invoke(HandSide.Left, leftPinchPoint);
            if (rightStarted)
                PinchStarted?.Invoke(HandSide.Right, rightPinchPoint);
            if (leftEnded)
                PinchEnded?.Invoke(HandSide.Left);
            if (rightEnded)
                PinchEnded?.Invoke(HandSide.Right);

            if (isActive && !wasActive)
                GestureStarted?.Invoke(next);
            else if (!isActive && wasActive)
                GestureEnded?.Invoke(next);

            StateChanged?.Invoke(next);
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
