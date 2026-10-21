using DigitalLove.Game.Spaceships;
using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class UniverseGhostController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UniverseView universeView;
        [SerializeField] private UniverseGhostView ghostView;
        [SerializeField] private TwoHandGestureInput gestureInput;
        [SerializeField] private SpaceshipsSpawner spaceshipsSpawner;

        [Header("Scale")]
        [SerializeField] private float minHandSeparation = 0.06f;
        [SerializeField] private float scaleGain = 2.5f;
        [SerializeField] private float enterRoomScale = 0.6f;
        [SerializeField] private float exitRoomScale = 0.45f;

        [Header("Bubble transform")]
        [SerializeField] private float translationGain = 1f;
        [SerializeField] private float translationDeadZone = 0.01f;
        [SerializeField] private float settleDistance = 0.01f;
        [SerializeField] private float translationSmoothSecs = 0.08f;
        [SerializeField] private float dragStartJumpLimit = 0.12f;
        [SerializeField] private float recenterDistance = 0.05f;
        [SerializeField] private float recenterHoldSecs = 0.35f;
        [SerializeField] private float viewChangeCooldownSecs = 0.35f;

        private enum SessionMode { None, Drag, Scale }
        private bool sessionActive;
        private SessionMode sessionMode;
        private bool suppressUntilRelease;
        private float cooldownUntil;
        private BubbleMapTransformSession bubbleTransform;
        private BubbleScaleSession bubbleScale;

        private void Awake()
        {
            bubbleTransform = new BubbleMapTransformSession(
                translationGain,
                translationDeadZone,
                settleDistance,
                translationSmoothSecs,
                dragStartJumpLimit);
            bubbleScale = new BubbleScaleSession(
                scaleGain,
                enterRoomScale,
                exitRoomScale,
                recenterDistance,
                recenterHoldSecs);
        }

        private void OnEnable()
        {
            universeView.TransitionCompleted += OnTransitionCompleted;
        }

        private void Start()
        {
            ghostView.Hide();
            cooldownUntil = Time.unscaledTime + viewChangeCooldownSecs;
        }

        private void Update()
        {
            if (universeView.IsTransitioning)
            {
                ghostView.Hide();
                return;
            }

            universeView.SetMapGestureBlocked(spaceshipsSpawner.IsAnyRouteEditing());
            if (spaceshipsSpawner.IsAnyRouteEditing())
            {
                if (sessionActive)
                    EndSession();
                ghostView.Hide();
                return;
            }

            if (Time.unscaledTime < cooldownUntil)
            {
                ghostView.Hide();
                return;
            }

            if (UpdateSuppression())
                return;

            universeView.SetMapGestureActive(gestureInput.IsAnyPinching && !universeView.IsMapGestureBlocked);
            if (!sessionActive)
                universeView.SetScaleFeedback(0f);

            UpdateFeedback();

            if (UpdateActiveSession())
                return;

            TryStartSession();
        }

        private bool UpdateSuppression()
        {
            if (!suppressUntilRelease)
                return false;

            ghostView.Hide();
            if (!gestureInput.IsAnyPinching)
                suppressUntilRelease = false;
            return true;
        }

        private bool UpdateActiveSession()
        {
            if (!sessionActive)
                return false;

            if (sessionMode == SessionMode.Scale && !gestureInput.IsActive)
            {
                EndSession();
                return true;
            }

            if (sessionMode == SessionMode.Drag && (gestureInput.IsActive || !gestureInput.IsSingleHandPinching))
            {
                EndSession();
                suppressUntilRelease = true;
                return true;
            }

            UpdateSessionState();
            return true;
        }

        private void TryStartSession()
        {
            if (!universeView.HasRoomAnchor)
                return;

            if (gestureInput.IsActive)
            {
                float handDistance = gestureInput.HandDistance;
                if (handDistance < minHandSeparation)
                    return;

                StartSession(SessionMode.Scale, handDistance);
                return;
            }

            bool insideBubble = Vector3.Distance(gestureInput.DragPoint, universeView.CenterWorld)
                <= universeView.BubbleRadiusWorld * 0.5f;
            if (universeView.IsMini && gestureInput.IsSingleHandPinching && insideBubble)
                StartSession(SessionMode.Drag, 0f);
        }

        private void StartSession(SessionMode mode, float handDistance)
        {
            sessionActive = true;
            sessionMode = mode;
            spaceshipsSpawner.SetViewTransition(true);
            bubbleTransform.Begin(universeView, gestureInput.DragPoint);
            if (mode == SessionMode.Scale)
                bubbleScale.Begin(universeView.Scale, handDistance);
        }

        private void UpdateSessionState()
        {
            if (sessionMode == SessionMode.Drag)
            {
                bubbleTransform.Update(universeView, gestureInput.DragPoint);
                return;
            }

            BubbleScaleAction action = bubbleScale.Update(
                universeView,
                gestureInput.HandDistance,
                Time.deltaTime);

            if (action == BubbleScaleAction.Recenter)
                RecenterBubble();
            else if (action == BubbleScaleAction.EnterRoom)
                Commit(false);
            else if (action == BubbleScaleAction.ExitRoom)
                Commit(true);
        }

        private void UpdateFeedback()
        {
            bool leftPinching = gestureInput.IsLeftPinching;
            bool rightPinching = gestureInput.IsRightPinching;

            if (!leftPinching && !rightPinching)
            {
                ghostView.Hide();
                return;
            }

            ghostView.Show(
                gestureInput.LeftPinchPoint,
                gestureInput.RightPinchPoint,
                leftPinching,
                rightPinching,
                gestureInput.HandDistanceSpeed);
        }

        private void RecenterBubble()
        {
            EndSession();
            universeView.ResetBubbleMap();
            universeView.SetScaleFeedback(0f);
            suppressUntilRelease = true;
        }

        private void Commit(bool toBubble)
        {
            sessionActive = false;
            sessionMode = SessionMode.None;
            suppressUntilRelease = true;
            bubbleTransform.Commit(universeView);
            ghostView.Hide();
            universeView.SetBubbleView(toBubble, false);
        }

        private void EndSession()
        {
            sessionActive = false;
            sessionMode = SessionMode.None;
            bubbleTransform.Settle(universeView);
            ghostView.Hide();
            spaceshipsSpawner.SetViewTransition(false);
        }

        private void OnTransitionCompleted(bool toBubble)
        {
            spaceshipsSpawner.SetViewTransition(false);
            spaceshipsSpawner.RefreshGrabbablesAtStation();
            cooldownUntil = Time.unscaledTime + viewChangeCooldownSecs;

            if (suppressUntilRelease && !gestureInput.IsAnyPinching)
                suppressUntilRelease = false;
        }

        private void OnDisable()
        {
            universeView.TransitionCompleted -= OnTransitionCompleted;
            if (sessionActive)
                EndSession();
            ghostView.Hide();
        }
    }
}
