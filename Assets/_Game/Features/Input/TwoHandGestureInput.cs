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
        [SerializeField, Min(0f)] private float releaseGraceSecs = 0.4f;

        [Header("Scale out")]
        [SerializeField, Min(0.01f)] private float scaleOutMinDistance = 0.08f;
        [SerializeField, Range(0f, 1f)] private float scaleOutMaxRatio = 0.8f;

        [Header("Debug")]
        [SerializeField] private bool logGesture;

        private bool paused;
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
        private bool isHandBusy;
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
        public float ScaleOutMinDistance => scaleOutMinDistance;
        public float ScaleOutMaxRatio => scaleOutMaxRatio;
        public event Action<TwoHandGestureState> StateChanged;
        public event Action<TwoHandGestureState> GestureStarted;
        public event Action<TwoHandGestureState> GestureEnded;

        // Paused by the app (PauseController stops time): skip, and start clean on resume so a dwell
        // held behind the system menu cannot fire.
        private void Update()
        {
            if (Time.timeScale <= 0f)
            {
                paused = true;
                return;
            }

            if (paused)
            {
                paused = false;
                ResetGesture();
            }

            bool wasActive = isActive;
            ReadHands();
            UpdateDistanceSpeed(isLeftPinching && isRightPinching);
            UpdateActive(isLeftPinching && isRightPinching, hasLeftHand && hasRightHand);
            Publish();
            LogIfNeeded(wasActive);
        }

        private void OnEnable() => ResetGesture();

        private void ResetGesture()
        {
            isActive = false;
            bothPinchingSince = -1f;
            lastBothPinchTime = 0f;
            previousHandDistance = 0f;
            handDistanceSpeed = 0f;
            hasPreviousHandDistance = false;
            wasLeftPinching = false;
            wasRightPinching = false;
            leftStarted = false;
            rightStarted = false;
            leftEnded = false;
            rightEnded = false;
        }

        private void ReadHands()
        {
            hasLeftHand = TryGetPinchPoint(leftHandSource, out Vector3 leftPoint);
            hasRightHand = TryGetPinchPoint(rightHandSource, out Vector3 rightPoint);
            isHandBusy = IsHandBusy();

            if (hasLeftHand)
                leftPinchPoint = leftPoint;
            if (hasRightHand)
                rightPinchPoint = rightPoint;

            isLeftPinching = hasLeftHand && !isHandBusy && IsPinching(leftHandSource);
            isRightPinching = hasRightHand && !isHandBusy && IsPinching(rightHandSource);
            handDistance = Vector3.Distance(leftPinchPoint, rightPinchPoint);
        }

        private void LogIfNeeded(bool wasActive)
        {
            if (!logGesture
                || !(leftStarted || rightStarted || leftEnded || rightEnded || isActive != wasActive))
            {
                return;
            }

            Debug.Log(
                $"[TwoHandGesture] L={hasLeftHand}/{isLeftPinching} R={hasRightHand}/{isRightPinching} " +
                $"busy={isHandBusy} active={isActive} distance={handDistance:F3} speed={handDistanceSpeed:F3}",
                this);
        }

        private void UpdateDistanceSpeed(bool bothPinching)
        {
            if (!bothPinching || Time.deltaTime <= 0f)
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
                bool onePinching = isLeftPinching || isRightPinching;
                if (isActive && (onePinching || !isTracked
                    || Time.unscaledTime - lastBothPinchTime > releaseGraceSecs))
                {
                    isActive = false;
                }
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
                new HandPinchState(isLeftPinching, leftPinchPoint, leftPinchStartPoint),
                new HandPinchState(isRightPinching, rightPinchPoint, rightPinchStartPoint),
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
